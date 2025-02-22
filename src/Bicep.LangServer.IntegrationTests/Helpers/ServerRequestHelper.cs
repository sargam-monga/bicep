// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bicep.Core.Navigation;
using Bicep.Core.Text;
using Bicep.Core.Workspaces;
using Bicep.LangServer.IntegrationTests.Helpers;
using Bicep.LanguageServer.Utils;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Bicep.LangServer.IntegrationTests
{
    public class FileRequestHelper
    {
        private readonly ILanguageClient client;
        private readonly BicepFile bicepFile;

        public FileRequestHelper(ILanguageClient client, BicepFile bicepFile)
        {
            this.client = client;
            this.bicepFile = bicepFile;
        }

        public async Task<ImmutableArray<CompletionList>> RequestCompletions(IEnumerable<int> cursors)
        {
            var completions = new List<CompletionList>();
            foreach (var cursor in cursors)
            {
                var completionList = await RequestCompletion(cursor);

                completions.Add(completionList);
            }

            return completions.ToImmutableArray();
        }

        public async Task<CompletionList> RequestCompletion(int cursor)
        {
            return await client.RequestCompletion(new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier(bicepFile.FileUri),
                Position = TextCoordinateConverter.GetPosition(bicepFile.LineStarts, cursor)
            });
        }

        public BicepFile ApplyCompletion(CompletionList completions, string label, params string[] tabStops)
        {
            completions.Should().ContainSingle(x => x.Label == label);

            return ApplyCompletion(completions.Single(x => x.Label == label), tabStops);
        }

        public BicepFile ApplyCompletion(CompletionItem completion, params string[] tabStops)
        {
            var start = PositionHelper.GetOffset(bicepFile.LineStarts, completion.TextEdit!.TextEdit!.Range.Start);
            var end = PositionHelper.GetOffset(bicepFile.LineStarts, completion.TextEdit!.TextEdit!.Range.End);
            var textToInsert = completion.TextEdit!.TextEdit!.NewText;

            // the completion handler returns tabs. convert to double space to simplify printing.
            textToInsert = textToInsert.Replace("\t", "  ");

            // always expect this mode for now
            completion.InsertTextMode.Should().Be(InsertTextMode.AdjustIndentation);

            switch (completion.InsertTextFormat)
            {
                case InsertTextFormat.Snippet:
                    // replace default tab stop with the custom cursor format we use in this test suite
                    textToInsert = textToInsert.Replace("$0", "|");
                    for (var i = 0; i < tabStops.Length; i++)
                    {
                        textToInsert = Regex.Replace(textToInsert, $"\\${i + 1}", tabStops[i]);
                        textToInsert = Regex.Replace(textToInsert, $"\\${{{i + 1}:[^}}]+}}", tabStops[i]);
                    }
                    break;
                case InsertTextFormat.PlainText:
                    textToInsert = textToInsert + "|";
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var originalFile = bicepFile.ProgramSyntax.ToTextPreserveFormatting();
            var replaced = originalFile.Substring(0, start) + textToInsert + originalFile.Substring(end);

            return SourceFileFactory.CreateBicepFile(bicepFile.FileUri, replaced);
        }
    }

    public class ServerRequestHelper
    {
        private readonly TestContext testContext;
        private readonly AsyncLazy<MultiFileLanguageServerHelper> languageServerHelperLazy;

        public ServerRequestHelper(TestContext testContext, SharedLanguageHelperManager server)
        {
            this.testContext = testContext;
            this.languageServerHelperLazy = new(() => server.GetAsync(), new(new JoinableTaskContext()));
        }

        public ServerRequestHelper(TestContext testContext, MultiFileLanguageServerHelper helper)
        {
            this.testContext = testContext;
            this.languageServerHelperLazy = new(() => Task.FromResult(helper), new(new JoinableTaskContext()));
        }

        public async Task<FileRequestHelper> OpenFile(string text)
            => await OpenFile(
                new Uri($"file://{Guid.NewGuid():D}/{testContext.TestName}/main.bicep"),
                text);

        public async Task<FileRequestHelper> OpenFile(Uri fileUri, string text)
        {
            var bicepFile = SourceFileFactory.CreateBicepFile(fileUri, text);

            var helper = await languageServerHelperLazy.GetValueAsync();
            await helper.OpenFileOnceAsync(testContext, bicepFile);

            return new FileRequestHelper(helper.Client, bicepFile);
        }
    }
}
