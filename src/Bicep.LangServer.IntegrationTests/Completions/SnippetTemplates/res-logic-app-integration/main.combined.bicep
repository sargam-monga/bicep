// $1 = logicApp
// $2 = 'name'
// $3 = location

resource logicApp 'Microsoft.Logic/integrationAccounts@2019-05-01' = {
  name: 'name'
  location: resourceGroup().location
}
// Insert snippet here

