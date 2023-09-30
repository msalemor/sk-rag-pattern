param appName string
param resourceGroupName string
param location string = "eastus" 
param containerImage string
param containerRegistryName string
param containerRegistryUsername string
param containerRegistryPassword string

param domain string
param env string = "poc"

var hname = "${domain}-{env}"
var nhname = "${domain}{env}"

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: '${hname}-plan'
  location: location
  properties: {
    name: '${hname}-plan'
    reserved: true
    sku: {
      name: 'B1'
      tier: 'Basic'
      size: 'B1'
      family: 'B'
      capacity: 1
    }
  }
}

resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        },
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${containerRegistryName}.azurecr.io'
        },
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: containerRegistryUsername
        },
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: containerRegistryPassword
        }
      ]
    }
  }
}
