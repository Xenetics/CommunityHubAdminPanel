# CommunityHubAdminPanel
Administration panel for a Library focused digital community hub

The App repo is located at https://github.com/Xenetics/CommunityHub

## Config file info
When you open the admin panel the first time it will immediatly close after popping up the config file that looks like this.  

Empty image here

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

When completed the config should look like this.

Completed image

## Fields to Modify  
The following are fields that you will want to modify to customize your admin panel.  

<b>PinTypes</b> : Names of the pintypes that you want in the app	AdminPanel 
### AdminUtilities.cs  
<b>Organization</b> : Organisations that have admin privilages AdminPanel  
  
### Product.cs  
<b>ProductOrg</b> : Organizations that have products in the hub	AdminPanel  
  
### QRCode.cs  
<b>QRTypes</b> : Organizations that can be connected to a QR Code	AdminPanel  	
  

  
### UserUtilities.cs  
<b>containerName</b> : Container for the Users of the app can be created in azure portal or microsoft azure storage explorer	AdminPanel  
