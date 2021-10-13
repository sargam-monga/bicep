// $1 = workSpace
// $2 = 'name'
// $3 = location
// $4 = 'friendlyName'

resource workSpace 'Microsoft.DesktopVirtualization/workspaces@2021-07-12' = {
  name: 'name'
  location: resourceGroup().location
  properties: {
    friendlyName: 'friendlyName'
  }
}
// Insert snippet here

