# CommunityHubAdminPanel
Administration panel for a Library focused digital community hub.

The App repo is located at https://github.com/Xenetics/CommunityHub

## Azure
For this project you will need to create a storage account on Azure. Below is a link to a tutorial for doing this that is kept up to date.  
[Azure Tutorial](https://docs.microsoft.com/en-us/azure/storage/common/storage-create-storage-account)  

## Config file info
When you open the admin panel the first time it will immediatly close after popping up the config file that looks like this.  

![alt text](https://raw.githubusercontent.com/xenetics/CommunityHubAdminPanel/master/ExampleImages/EmptyConfig.png)

<b>StorageKey</b> : Azure storage key can be found in the azure portal after you create storage account	AdminPanel  
<b>POIContainer</b> : Azure Blob Container for map points of interest  
<b>QRCodeContainer</b> : Azure Blob Container for QR Codes  
<b>EventsContainer</b> : Azure Table for calandar events  
<b>ProductsContainer</b> : Azure Blob Container for Products  
<b>TriviaContainer</b> : Azure Blob Container for Trivia Questions  
<b>UserContainer</b> : Azure Blob Container for Users  
<b>AdminContainer</b> : Azure Blob Container for Admins  
<b>SierraUrl</b> : Library Sierra server URL	AdminPanel  
<b>SierraSecret</b> : Library Sierra general API Key	AdminPanel  
<b>AdminName</b> : The name of your first super admin. Will be deleted upon reopening the admin panel  
<b>AdminPass</b> : The password for your first super admin. Will be deleted upon reopening the admin panel  

When completed the config should look like this. Note the AdminName and AdminPass as they will be deleted from the file.

![alt text](https://raw.githubusercontent.com/xenetics/CommunityHubAdminPanel/master/ExampleImages/CompleteConfig.png)  

Once you have filled in the config and saved, reopen the Anmin panel and login using the AdminName and AdminPass you set. 

## Fields to Modify  
The following are fields that you will want to modify to customize your admin panel.  

### AdminPanel.cs
<b>PinTypes</b> : Names of the pintypes that you want in the app	AdminPanel  
### AdminUtilities.cs  
<b>Organization</b> : Organisations that have admin privilages AdminPanel  
### Product.cs  
<b>ProductOrg</b> : Organizations that have products in the hub	AdminPanel  
### QRCode.cs  
<b>QRTypes</b> : Organizations that can be connected to a QR Code	AdminPanel   


All contents of repository are considered fair use under MIT licensing.
