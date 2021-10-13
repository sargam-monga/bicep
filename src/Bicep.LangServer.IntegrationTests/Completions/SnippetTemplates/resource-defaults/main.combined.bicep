// $1 = nsg
// $2 = Microsoft.Aad/domainServices@2021-05-01
// $3 = 'testResource'
// $4 = location

resource nsg 'Microsoft.Aad/domainServices@2021-05-01' = {
  name: 'testResource'
  location: 'testLocation'
  properties: {
    
  }
}// Insert snippet here

