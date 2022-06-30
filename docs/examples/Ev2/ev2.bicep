@secure()
param ev2Config string

@scopeBind(true)
param skuName string
param location string

import ev2Extension as ev2{
  ev2Config: ev2Config
  namespace: 'default'
}

resource storageTest 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: 'storage Test'
  location: location
  sku: {
    name: skuName
  }
  kind: 'BlobStorage'
}

var rolloutType = 'Minor'
var env = 'Int'
var displayName = 'resource test 001'
var serviceGroup = 'Microsoft.M365.PP.eersapibuildouttest'
var subscriptionKey = '123456'

resource shellExtension01 'shellExtension/ev2Extension@v1' = {
  skipDeleteAfterExecution: true
  maxExecutionTime: 'P10H'
  package: 'junk'
  useFallBackLocations: false 
  command: [
    'sss'
  ]
  metadata: {
    name: 'name'
  }
}

resource httpExtension 'extension/ev2Extension@v1' = {
  type: 'Microsoft.SkypeECS/__ecsExtension__'
  payloadProperties: {
    'ecsAgents': {
      'value': 'Testsmbabuildout'
    }
    'ecsClient': {
      'value': 'CloudConfiguration'
    }
    'guestAgents': {
      'value': {
        'CloudConfiguration': [
          'Global'
          'ServiceGlobal'
        ]
      }
    }
    'populateCloudFilter': {
      'value': 'true'
    }
    'requestIdentifiers': {
      'value': {
        'GeographyName': '__GEOGRAPHY_NAME__'
        'RegionName': '__REGION_NAME__'
        'BuildVersion': '__BUILD_VERSION__'
        'Region': '__REGION_NAME__'
      }
    }
    'serviceTreeId': {
      'value': '38fc030e-fdfb-4070-af15-b9ef203ac5cb'
    }
  }
  maxExecutionTime: 'P1D'
  authenticationProperties: {}
  metadata: {
    name: 'httpExtension'
  }
  authenticationType: 'SystemCertificateAuthentication'
}

resource RegionAgnosticResource 'resource/ev2Extension@v1' = {
  orchestratedSteps: [
    {
      resourceRef: storageTest
      actions: [
        'deploy'
      ]
      resourceGroupName: 'TestName'
    }
    {
      resourceRef: httpExtension
      actions: [
        'deploy'
      ]
      dependsOn: [
        storageTest
      ]
      resourceGroupName: 'TestName'
    }
    
  ]
  displayName: displayName
  subscriptionKey: subscriptionKey
  metadata: {
    name: 'RegionAgnosticResource'
  }
  rolloutType: rolloutType
  environment: env
  serviceIdentifier: '38fc030e-fdfb-4070-af15-b9ef203ac5cb'
  serviceGroup: serviceGroup
}

