# CommunityHubAdminPanel
Administration panel for a Library focused digital community hub

The App repo is located at https://github.com/Xenetics/CommunityHub

## Required fields to modify for your

AdminPanel.cs
azureHelper param : Azure storage key can be found in the azure portal after you create storage account	AdminPanel
POIContainer : Container for the points of interest can be created in azure portal or microsoft azure storage explorer	AdminPanel
PinTypes : Names of the pintypes that you want in the app	AdminPanel
QRCodeContainer : Container for the QR Codes can be created in azure portal or microsoft azure storage explorer	AdminPanel
EventsContainer : Container for the Calander Events can be created in azure portal or microsoft azure storage explorer	AdminPanel
ProductsContainer : Container for the Products can be created in azure portal or microsoft azure storage explorer	AdminPanel
triviaContainer : Container for the trivia can be created in azure portal or microsoft azure storage explorer	AdminPanel
triviaPartition : General partition for the trivia table	AdminPanel

AdminUtilities.cs
containerName : Container for the Administration accounts can be created in azure portal or microsoft azure storage explorer	AdminPanel	
Organization : Organisations that have admin privilages	AdminPanel

Product.cs
ProductOrg : Organizations that have products in the hub	AdminPanel

QRCode.cs
QRTypes : Organizations that can be connected to a QR Code	AdminPanel	

RestHelper.cs
m_url : Library Sierra server URL	AdminPanel	

RestHelper.cs
m_authSecret : Library Sierra general API Key	AdminPanel	

UserUtilities.cs
containerName : Container for the Users of the app can be created in azure portal or microsoft azure storage explorer	AdminPanel
