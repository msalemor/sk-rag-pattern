param location string = 'eastus'
param containerImage string = 'amcr.azurecr.io/azure-vote-front:v1'
param containerRegistryName string = 'amcr.azurecr.io'
param containerRegistryUsername string
param openAIEndpoint string
@secure()
param openAIKey string
param gptEndpoint string
param adaDeploymentName string = 'ada'
param gptDploymentName string = 'gpt-16'

param domain string = 'contoso'
param projectID string = 'ragmin'
param env string = 'poc'

var hname = '${domain}-${projectID}${env}'
var nhname = '${domain}${projectID}${env}'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
  }
}
output containerRegistryName string = containerRegistry.name
output containerRegistryUsername string = containerRegistry.properties.loginServer
//output containerRegistryPassword string = containerRegistry.listCredentialsResult.passwords[0].value

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
  name: 'app1-${hname}-webapp'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
      appSettings: [

        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${containerRegistryName}.azurecr.io'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: containerRegistryUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: ''
        }
        {
          name: 'API_KEY'
          value: openAIKey
        }
        {
          name: 'GPT_ENDPOINT'
          value: gptEndpoint
        }
        {
          name: 'GPT_DEPLOYMENT_NAME'
          value: gptDploymentName
        }
        {
          name: 'ADA_DEPLOYMENT_NAME'
          value: adaDeploymentName
        }
      ]
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${hname}-appi'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}
