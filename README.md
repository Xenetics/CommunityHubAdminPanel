# CommunityHubAdminPanel
Administration panel for a Library focused digital community hub

The App repo is located at https://github.com/Xenetics/CommunityHub

## Required fields to modify for your

### AdminPanel.cs  
<b>azureHelper param</b> : Azure storage key can be found in the azure portal after you create storage account	AdminPanel  
<b>POIContainer</b> : Container for the points of interest can be created in azure portal or microsoft azure storage explorer	AdminPanel  
<b>PinTypes</b> : Names of the pintypes that you want in the app	AdminPanel  
<b>QRCodeContainer</b> : Container for the QR Codes can be created in azure portal or microsoft azure storage explorer	AdminPanel  
<b>EventsContainer</b> : Container for the Calander Events can be created in azure portal or microsoft azure storage explorer	AdminPanel  
<b>ProductsContainer</b> : Container for the Products can be created in azure portal or microsoft azure storage explorer	AdminPanel  
<b>triviaContainer</b> : Container for the trivia can be created in azure portal or microsoft azure storage explorer	AdminPanel  
<b>triviaPartition</b> : General partition for the trivia table	AdminPanel  
  
### AdminUtilities.cs  
<b>containerName</b> : Container for the Administration accounts can be created in azure portal or microsoft azure storage explorer	AdminPanel  
<b>Organization</b> : Organisations that have admin privilages	AdminPanel  
  
### Product.cs  
<b>ProductOrg</b> : Organizations that have products in the hub	AdminPanel  
  
### QRCode.cs  
<b>QRTypes</b> : Organizations that can be connected to a QR Code	AdminPanel  	
  
### RestHelper.cs  
<b>m_url</b> : Library Sierra server URL	AdminPanel  	
  
### RestHelper.cs  
<b>m_authSecret</b> : Library Sierra general API Key	AdminPanel  	
  
### UserUtilities.cs  
<b>containerName</b> : Container for the Users of the app can be created in azure portal or microsoft azure storage explorer	AdminPanel  
