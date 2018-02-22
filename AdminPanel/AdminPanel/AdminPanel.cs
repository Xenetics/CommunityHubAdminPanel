using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text.RegularExpressions;
using QRCoder;

namespace AdminPanel
{
    public partial class AdminPanel : Form
    {
        // A object which allows interaction with the azure databases
        public AzureHelper azureHelper;

        // States
        public enum MainState { Login, PointsRedeem, UserDB, ProductCatalog, Events, Pins, QRCodes, AdminDB, Trivia }
        // Current state of the panel
        public MainState CurState;

        public AdminPanel()
        {
            InitializeComponent();
            string version = Application.ProductVersion;
            this.Text = String.Format("Admin panel - {0}", version);
        }

        // Executed on load of the program
        private void AdminPanel_Load(object sender, EventArgs e)
        {
            // Init anything

            // Create Azure helper and pass in storage key string
            azureHelper = new AzureHelper("AZURE BLOBSTORE STORAGE KEY STRING"); // REQUIRED-FIELD : Azure storage key can be found in the azure portal after you create storage account

            // Create various lists for dropdowns on the panel pages
            TypesDropdown.Items.AddRange(PinTypes.ToArray());
            QREnumNames = new List<string>(Enum.GetNames(typeof(QRCode.QRTypes)).ToArray());
            QRTypeDropDown.Items.AddRange(QREnumNames.ToArray());
            EventOrgDropdown.DataSource = new List<string>(Enum.GetNames(typeof(AdminUtilities.Organization)).ToArray());
            AdminOrgDropdown.DataSource = new List<string>(Enum.GetNames(typeof(AdminUtilities.Organization)).ToArray());
            AdminClearenceDropdown.DataSource = new List<string>(Enum.GetNames(typeof(AdminUtilities.Clearance)).ToArray());
            ProductOrgDropdown.DataSource = new List<string>(Enum.GetNames(typeof(AdminUtilities.Organization)).ToArray());
            TriviaOrganization_Dropdown.DataSource = new List<string>(Enum.GetNames(typeof(AdminUtilities.Organization)).ToArray());

            // Set Initial State
            NewMainState(MainState.Login);
        }

        // Main state of the panel
        private void NewMainState(MainState newState)
        {
            DisabledButtonClear();
            CurState = newState;
            ClearQRPanelValue();
            ClearProductPanelValues();
            switch (newState)
            {
                case MainState.Login:
                    TitleLabel.Text = "Login";
                    TabPanel.SelectedTab = LoginTab;
                    UserDB_BTN.Enabled = false;
                    ProductCatalog_BTN.Enabled = false;
                    Events_BTN.Enabled = false;
                    Pins_BTN.Enabled = false;
                    QRCode_BTN.Enabled = false;
                    Trivia_BTN.Enabled = false;
                    AdminDB_BTN.Enabled = false;
                    break;
                case MainState.UserDB:
                    TitleLabel.Text = "User Database";
                    TabPanel.SelectedTab = UserDBTab;
                    UserDB_BTN.Enabled = false;
                    DownloadUsers();
                    UserFieldsClear();
                    break;
                case MainState.ProductCatalog:
                    TitleLabel.Text = "Product Catalog";
                    TabPanel.SelectedTab = ProductTab;
                    ProductCatalog_BTN.Enabled = false;
                    ChangeProductState(ProductPanelStates.GENERATE);
                    DownloadProducts();
                    break;
                case MainState.Events:
                    TitleLabel.Text = "Events";
                    EventSetup();
                    SetEventState(EventState.Idle);
                    TabPanel.SelectedTab = EventsTab;
                    Events_BTN.Enabled = false;
                    DownloadMonthsEvents();
                    break;
                case MainState.Pins:
                    TitleLabel.Text = "Pin Locations";
                    TabPanel.SelectedTab = PinsTab;
                    Pins_BTN.Enabled = false;
                    DownloadPOIS();
                    break;
                case MainState.QRCodes:
                    TitleLabel.Text = "QR Codes";
                    TabPanel.SelectedTab = QRTab;
                    QRCode_BTN.Enabled = false;
                    DownloadQRCodes();
                    break;
                case MainState.AdminDB:
                    TitleLabel.Text = "Admin Database";
                    TabPanel.SelectedTab = AdminDBTab;
                    AdminDB_BTN.Enabled = false;
                    DownloadAdmins();
                    break;
                case MainState.Trivia:
                    TitleLabel.Text = "Trivia Database";
                    SetTriviaState(TriviaState.Idle);
                    TabPanel.SelectedTab = TriviaTab;
                    Trivia_BTN.Enabled = false;
                    DownloadTrivia();
                    break;
            }
        }

        // Disables and enable buttons on Side panel
        private void DisabledButtonClear()
        {
            UserDB_BTN.Enabled = true;
            ProductCatalog_BTN.Enabled = true;
            Events_BTN.Enabled = true;
            Pins_BTN.Enabled = true;
            Trivia_BTN.Enabled = true;
            QRCode_BTN.Enabled = true;
            if (AdminUtilities.ModificationClearenceCheck(AdminUtilities.Clearance.Admin.ToString()))
            {
                AdminDB_BTN.Enabled = true;
            }
        }

        #region Left Panel Buttons
        private void UserDB_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.UserDB);
        }

        private void ProductCatalog_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.ProductCatalog);
        }

        private void Events_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.Events);
        }

        private void Pins_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.Pins);
        }

        private void QRCodes_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.QRCodes);
        }

        private void AdminDB_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.AdminDB);
        }

        private void Trivia_BTN_Click(object sender, EventArgs e)
        {
            NewMainState(MainState.Trivia);
        }
        #endregion

        // Turns on or off the loading panel to hide funcionality until ready
        private void LoadingOn(bool on)
        {
            LoadingPanel.BringToFront();
            LoadingPanel.Visible = on;
        }

        #region Login
        // Event for clicking the login button on login page
        private async void Login_BTN_Click(object sender, EventArgs e)
        {
            LoadingOn(true);
            switch (await AdminUtilities.CheckRemoteData(LoginUsernameInput.Text, LoginPasswordInput.Text, azureHelper.BlobClient))
            {
                case "Correct":
                    NewMainState(MainState.UserDB);
                    return;
                case "Incorrect":
                    MessageBox.Show("Either username or password is incorrect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadingOn(false);
                    break;
                case "NonExistant":
                    MessageBox.Show("User does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadingOn(false);
                    break;
                case "Empty":
                    MessageBox.Show("Please enter both a username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadingOn(false);
                    return;
            }
            LoadingOn(false);
        }
        
        // Event occurs when key is pressed with username input in focus
        private void LoginUsernameInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            // attemps login when enter is pressed
            if(e.KeyChar == (char)Keys.Return)
            {
                Login_BTN_Click(sender, e);
            }
        }

        // Event occurs when key is pressed with password input in focus
        private void LoginPasswordInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            // attemps login when enter is pressed
            if (e.KeyChar == (char)Keys.Return)
            {
                Login_BTN_Click(sender, e);
            }
        }
        #endregion

        #region AdminDB
        /// <summary> List of admins downloaded from the blobstore </summary>
        public List<AdminUtilities.AdminData> Admins;

        // Downloads a list of the admins user information (Requires top clearence) 
        private async void DownloadAdmins()
        {
            LoadingOn(true);
            AdminListBox.Items.Clear();
            Admins = new List<AdminUtilities.AdminData>();

            CloudBlobContainer container = azureHelper.BlobClient.GetContainerReference(AdminUtilities.containerName);
            BlobContinuationToken blobConToken = null;
            BlobResultSegment result;

            do
            {
                result = await container.ListBlobsSegmentedAsync(blobConToken);
                blobConToken = result.ContinuationToken;
            } while (blobConToken != null);

            List<string> blobs = new List<string>();
            blobs.AddRange(result.Results.Cast<CloudBlockBlob>().Select(b => b.Name));
            foreach (string blob in blobs)
            {
                Admins.Add(await AdminUtilities.GetRemoteData(blob, azureHelper.BlobClient));
            }

            AdminListBox.Items.Add(String.Format("~ {0, -15}| {1, -32} | {2, -12} | {3, -12} |\t", new string[] { "Username", "Password", "Admin Type", "Clearance Level" }));

            foreach (AdminUtilities.AdminData admin in Admins)
            {
                AdminListBox.Items.Add(admin.ToString());
            }
            AdminListBox.Items.Add("<Add New>");
            LoadingOn(false);
        }

        // Event for Create / Delete admin button
        private async void CreateAdmin_BTN_Click(object sender, EventArgs e)
        {
            LoadingOn(true);
            if (CreateAdminBTN.Text == "Create")
            {
                await AdminUtilities.CreateremoteData(AdminUsernameInput.Text, AdminPasswordInput.Text, AdminOrgDropdown.Text, AdminClearenceDropdown.Text, azureHelper.BlobClient);
            }
            else if (CreateAdminBTN.Text == "Delete")
            {
                await azureHelper.DeleteBlob(AdminUtilities.containerName, Admins[AdminListBox.SelectedIndex - 1].Username);
            }
            LoadingOn(false);
            DownloadAdmins();
            ClearAdminPanelValue();
        }

        // Event executed when the selected index of the list changes
        private void AdminList_SelectedIndex_Change(object sender, EventArgs e)
        {
            if (AdminListBox.SelectedIndex > 0)
            {
                if (AdminListBox.SelectedItem.ToString() == "<Add New>")
                {
                    CreateAdminBTN.Text = "Create";
                    ClearAdminPanelValue();
                    AdminInputs.Visible = true;
                }
                else
                {
                    CreateAdminBTN.Text = "Delete";
                    AdminInputs.Visible = false;
                }
            }
            else
            {
                if (CreateAdminBTN.Text == "Delete")
                {
                    CreateAdminBTN.Text = "Create";
                    ClearAdminPanelValue();
                    AdminInputs.Visible = true;
                }
            }
        }

        // Clears the input fields and defaults the dropdowns on the Admin DB page
        private void ClearAdminPanelValue()
        {
            AdminUsernameInput.Text = "";
            AdminPasswordInput.Text = "";
            AdminOrgDropdown.SelectedIndex = 0;
            AdminClearenceDropdown.SelectedIndex = 0;
        }

        // event occures when any key is pressed while Admin list box is in focus
        private void AdminListBox_KeyPressed(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region Users Page
        /// <summary> Currently Selected User data in the UserDB panel </summary>
        public UserUtilities.UserData currentUser;
        /// <summary> List of users downloaded from the blob storage </summary>
        public List<UserUtilities.UserData> Users;
        /// <summary> Array of account status types </summary>
        public string[] AccountStatusTypes = new string[] { "Active", "Banned", "TempBan[0-9]", "Deactivated" };

        // Event for User search button
        private async void UserSearchButton_Click(object sender, EventArgs e)
        {
            currentUser = await UserUtilities.GetRemoteData(UserLibCardInput.Text, azureHelper.BlobClient);
            UserDisplayBox.Lines = new string[] {   "Username: " + currentUser.Username,
                                                    "Email: " + currentUser.EMail,
                                                    "Current Points: " + currentUser.CurrentPoints,
                                                    "Total Points: " + currentUser.TotalPoints,
                                                    "Account Status: " + currentUser.Active};
        }

        // Downloads the users for use in the User DB panel
        private async void DownloadUsers()
        {
            LoadingOn(true);
            UserList.Items.Clear();
            Users = new List<UserUtilities.UserData>();

            CloudBlobContainer container = azureHelper.BlobClient.GetContainerReference(UserUtilities.containerName);
            BlobContinuationToken blobConToken = null;
            BlobResultSegment result;

            do
            {
                result = await container.ListBlobsSegmentedAsync(blobConToken);
                blobConToken = result.ContinuationToken;
            } while (blobConToken != null);

            List<string> blobs = new List<string>();
            blobs.AddRange(result.Results.Cast<CloudBlockBlob>().Select(b => b.Name));
            foreach (string blob in blobs)
            {
                Users.Add(await UserUtilities.GetRemoteData(blob, azureHelper.BlobClient));
            }

            UserList.Items.Add(String.Format("~{0, -15} | {1, -32} | {2, -12} | {3, -12} | {4, -16} |\t", new string[] { "User Name", "EMail", "Points", "Total Points", "Status" }));

            foreach (UserUtilities.UserData user in Users)
            {
                UserList.Items.Add(user.ToString());
            }
            LoadingOn(false);
        }

        // Event executed when selection of a user in the user listbox changes
        private void UserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UserList.SelectedIndex > 0)
            {
                currentUser = Users[UserList.SelectedIndex - 1];
                UserLibCardInput.Text = currentUser.CardNumber;
                UserDisplayBox.Lines = new string[] {   "Username: " + currentUser.Username,
                                                    "Email: " + currentUser.EMail,
                                                    "Current Points: " + currentUser.CurrentPoints,
                                                    "Total Points: " + currentUser.TotalPoints,
                                                    "Account Status: " + currentUser.Active};
            }
        }

        // Used for changing the status of the user in the game (ie banned, activated)
        private void ChangeAccountStatus() 
        {
            // TODO: Will maybe want - ChangeAccountStatus()
        }

        // event occures when any key is pressed while user list box is in focus
        private void UserList_KeyPressed(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        // Reset the users password if forgotten
        private async void PasswordReset_BTN(object sender, EventArgs e)
        {
            if(currentUser.CardNumber.Length >= 0)
            {
                if(MessageBox.Show("Are you sure you would like to reset this password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk) == DialogResult.OK)
                {
                    LoadingOn(true);
                    char[] password = currentUser.CardNumber.ToArray();
                    Array.Reverse(password);
                    currentUser.Password = new string(password);

                    await azureHelper.ReplaceBlobContents(UserUtilities.containerName, currentUser.CardNumber, UserUtilities.UserDataToString(currentUser));
                    DownloadUsers();
                    UserFieldsClear();
                }
            }
            else
            {
                MessageBox.Show("No user is selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Change the users Library card number
        private async void ChangeLibraryCard_BTN(object sender, EventArgs e)
        {
            if (currentUser.CardNumber.Length >= 0)
            {
                if (!LibCardRegistered(NewCardNumber_Input.Text))
                {
                    if (MessageBox.Show("Are you sure you would like to change the users Library card number.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk) == DialogResult.OK)
                    {

                        LoadingOn(true);
                        string oldCard = currentUser.CardNumber;
                        currentUser.CardNumber = NewCardNumber_Input.Text;

                        await azureHelper.ReplaceBlobName(UserUtilities.containerName, oldCard, currentUser.CardNumber, UserUtilities.UserDataToString(currentUser));
                        DownloadUsers();
                        UserFieldsClear();
                    }
                }
                else
                {
                    MessageBox.Show("Card number already in use", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("No user is selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Checks if a user exists with Library card
        private bool LibCardRegistered(string cardnum)
        {
            foreach(UserUtilities.UserData ud in Users)
            {
                if(ud.CardNumber == cardnum)
                {
                    return true;
                }
            }
            return false;
        }

        // clears user list fields
        private void UserFieldsClear()
        {
            UserLibCardInput.Text = "";
            UserDisplayBox.Text = "";
            NewCardNumber_Input.Text = "";
        }

        #endregion

        #region Pins Page
        /// <summary> Azure container name for the Locations/pins/POIS for rewards </summary>
        public string POIContainer = "pois"; // REQUIRED-FIELD : Container for the points of interest can be created in azure portal or microsoft azure storage explorer
        /// <summary> Array of strings representing the pin types </summary>
        public List<string> PinTypes = new List<string> { "Generic", "Game", "Heritage", "Conservation", "MPL", "TOM", "Tokens" }; // REQUIRED-FIELD Names of the pintypes that you want in the app
        /// <summary> List of points downloaded from blobstore </summary>
        private List<Point> Points;
        /// <summary> Date regex for our date formatting </summary>
        Regex dateRegex = new Regex(@"^(\d)(\d)(,)(\d)(\d)(,)(\d)(\d)(\d)(\d)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// <summary> Minimum date for use in the panel </summary>
        public string MinDateString = "01,01,1970";
        /// <summary> Maximum date for use in the panel </summary>
        public string MaxDateString = "01,19,2037";

        // Event executed when the pinlist changes selected index on the pin panel.
        private void PinList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PinList.SelectedIndex > 0)
            {
                var item = PinList.SelectedItem;
                if (item.ToString() == "<Add New>")
                {
                    DeleteBTN.Enabled = false;
                    AddUpdateBTN.Text = "Add";
                    TypesDropdown.SelectedIndex = -1;
                    ClearPinPanelValue();
                }
                else
                {
                    DeleteBTN.Enabled = true;
                    AddUpdateBTN.Text = "Update";
                    TypesDropdown.SelectedIndex = PinTypes.FindIndex(Points[PinList.SelectedIndex - 1].type.Equals);
                    MessageInputBox.Text = Points[PinList.SelectedIndex - 1].label;
                    LatitudeInputBox.Text = Points[PinList.SelectedIndex - 1].latitude;
                    LongitudeInputBox.Text = Points[PinList.SelectedIndex - 1].longitude;
                    AddressInputBox.Text = Points[PinList.SelectedIndex - 1].address;
                    ValueInputBox.Text = Points[PinList.SelectedIndex - 1].value.ToString();
                    if (Points[PinList.SelectedIndex - 1].startDate != "")
                    {
                        StartDateInputBox.Text = Points[PinList.SelectedIndex - 1].startDate;
                    }
                    else
                    {
                        StartDateInputBox.Text = MinDateString;
                    }
                    if (Points[PinList.SelectedIndex - 1].endDate != "")
                    {
                        EndDateInputBox.Text = Points[PinList.SelectedIndex - 1].endDate;
                    }
                    else
                    {
                        EndDateInputBox.Text = MaxDateString;
                    }

                }
            }
        }

        // Event for the add/update pin button in the Locations panel
        private async void AddUpdateBTN_click(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(PinTypes[TypesDropdown.SelectedIndex]))
            {
                if (dateRegex.IsMatch(StartDateInputBox.Text) && dateRegex.IsMatch(EndDateInputBox.Text))
                {
                    string tempPOI = "";
                    tempPOI += "<TYPE>" + PinTypes[TypesDropdown.SelectedIndex];
                    tempPOI += "<LABEL>" + MessageInputBox.Text;
                    tempPOI += "<LAT>" + LatitudeInputBox.Text;
                    tempPOI += "<LONG>" + LongitudeInputBox.Text;
                    tempPOI += "<ADDRESS>" + AddressInputBox.Text;
                    tempPOI += "<VALUE>" + ValueInputBox.Text;
                    tempPOI += "<START>" + StartDateInputBox.Text;
                    tempPOI += "<END>" + EndDateInputBox.Text;
                    if (AddUpdateBTN.Text == "Add")
                    {
                        await azureHelper.PushBlob(POIContainer, tempPOI);
                        DownloadPOIS();
                    }
                    else
                    {
                        await azureHelper.ReplaceBlob(POIContainer, Points[PinList.SelectedIndex - 1].rawData, tempPOI);
                        DownloadPOIS();
                        AddUpdateBTN.Text = "Add";
                    }
                    TypesDropdown.SelectedIndex = -1;
                    ClearPinPanelValue();
                }
                else
                {
                    MessageBox.Show("Start Date or End Date is formatted wrong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Downloads all the points of interes data(pins)
        private async void DownloadPOIS()
        {
            LoadingOn(true);
            PinList.Items.Clear();
            Points = new List<Point>();

            CloudBlobContainer container = azureHelper.BlobClient.GetContainerReference(POIContainer);
            BlobContinuationToken blobConToken = null;
            BlobResultSegment result;

            do
            {
                result = await container.ListBlobsSegmentedAsync(blobConToken);
                blobConToken = result.ContinuationToken;
            } while (blobConToken != null);

            List<string> blobs = new List<string>();
            blobs.AddRange(result.Results.Cast<CloudBlockBlob>().Select(b => b.Name));
            foreach (string blob in blobs)
            {
                Point pd = Point.ParsePinData(blob);
                if (AdminUtilities.ViewClearenceCheck(pd.type))
                {
                    Points.Add(pd);
                }
            }

            PinList.Items.Add(String.Format("{0, -15} | {1, -15} | {2, -15} | {3, -5} | {4, -10} | {5, -10} | {6, -30} | {7, -30} |\t", new string[] { "Organization", "Latitude", "Longitude", "Tokens", "Start Date", "End Date", "Label", "Address" }));
            PinList.Items.AddRange(Points.ToArray());
            PinList.Items.Add("<Add New>");
            LoadingOn(false);
        }

        //  Event for the Delete button on the locations panel
        private async void DeleteBTN_Click(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(Points[PinList.SelectedIndex - 1].type))
            {
                DeleteBTN.Enabled = false;
                await azureHelper.DeleteBlob(POIContainer, Points[PinList.SelectedIndex - 1].rawData);
                DownloadPOIS();
                AddUpdateBTN.Text = "Add";
                TypesDropdown.SelectedIndex = -1;
                ClearPinPanelValue();
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Clears the values in the inputs of the locations panel and also defaults the dates
        private void ClearPinPanelValue()
        {
            MessageInputBox.Text = "";
            LatitudeInputBox.Text = "";
            LongitudeInputBox.Text = "";
            AddressInputBox.Text = "";
            ValueInputBox.Text = "";
            StartDateInputBox.Text = "01,01,1970";
            EndDateInputBox.Text = "01,19,2037";
        }

        // event occures when any key is pressed while Pin list box is in focus
        private void PinsList_KeyPressed(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region QR Codes
        /// <summary> Azure container name for the QRCodes for rewards </summary>
        public string QRCodeContainer = "qrcodes"; // REQUIRED-FIELD : Container for the QR Codes can be created in azure portal or microsoft azure storage explorer
        /// <summary> List of Downloaded QR codes for point distrobution </summary>
        private List<QRCode> QRCodes;
        /// <summary> List of string values that represent the enums for qr types </summary>
        private List<string> QREnumNames;
        /// <summary> Generates QR codes with given phrase </summary>
        private QRCodeGenerator QRGenerator = new QRCodeGenerator();
        /// <summary> The currently displayed QR code </summary>
        private QRCode DisplayedQR;
        /// <summary> The string path to where the qrcode is saved </summary>
        string BmpPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\QRcode.bmp";

        /// <summary> Downloads the QR code data </summary>
        private async void DownloadQRCodes()
        {
            LoadingOn(true);
            QRCodeList.Items.Clear();
            QRCodes = new List<QRCode>();

            CloudBlobContainer container = azureHelper.BlobClient.GetContainerReference(QRCodeContainer);
            BlobContinuationToken blobConToken = null;
            BlobResultSegment result;

            do
            {
                result = await container.ListBlobsSegmentedAsync(blobConToken);
                blobConToken = result.ContinuationToken;
            } while (blobConToken != null);

            List<string> blobs = new List<string>();
            blobs.AddRange(result.Results.Cast<CloudBlockBlob>().Select(b => b.Name));
            foreach (string blob in blobs)
            {
                QRCodes.Add(QRCode.QRParse(blob));
            }

            QRCodeList.Items.Add(String.Format("~{0, -11} | {1, -10} |\t", new string[] { "Type", "Token Value" }));

            foreach (QRCode qr in QRCodes)
            {
                QRCodeList.Items.Add(qr.ToString());
            }
            QRCodeList.Items.Add("<Add New>");
            LoadingOn(false);
        }

        // Event executed when the Generate or regenerate button is clicked on the QR panel
        private void GenRegen_BTN_Click(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(QRTypeDropDown.Text))
            {
                if (QRTypeDropDown.SelectedIndex > -1 && QRTokenValue.Text.Length > 0)
                {
                    if (!QRExists(QRTypeDropDown.SelectedItem.ToString(), QRTokenValue.Text))
                    {
                        DisplayedQR = new QRCode((QRCode.QRTypes)Enum.Parse(typeof(QRCode.QRTypes), QRTypeDropDown.SelectedItem.ToString()), int.Parse(QRTokenValue.Text));
                        QRCodeData QRData = QRGenerator.CreateQrCode(DisplayedQR.RawData, QRCodeGenerator.ECCLevel.Q);
                        DisplayedQR.Code = new QRCoder.QRCode(QRData);

                        QRPicture.Image = DisplayedQR.GetImage();

                        QRSave_BTN.Enabled = true;
                        QRDelete_BTN.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("QR code already exists", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
                else
                {
                    MessageBox.Show("Both type and content need to be defined", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Checks to see if the qr code already exists in database
        private bool QRExists(string _type, string _value)
        {
            QRCode.QRTypes type = (QRCode.QRTypes)Enum.Parse(typeof(QRCode.QRTypes), _type);
            int value = int.Parse(_value);

            foreach (QRCode qr in QRCodes)
            {
                if (qr.QRType == type && qr.TokenValue == value)
                {
                    return true;
                }
            }
            return false;
        }

        // Event executed when the Save QR button is clicked on QR panel
        private void QRSave_BTN_Click(object sender, EventArgs e)
        {
            LoadingOn(true);
            if (!QRExists(DisplayedQR.QRType.ToString(), DisplayedQR.TokenValue.ToString()))
            {
                FileStream fs = new FileStream(Path.GetDirectoryName(Application.ExecutablePath) + @"\QRcode" + DisplayedQR.TokenValue.ToString() + ".bmp", FileMode.Create);
                DisplayedQR.GetImage().Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                fs.Close();

                UploadQRCode("QRcode" + DisplayedQR.TokenValue.ToString() + ".bmp");
                ClearQRPanelValue();
            }
            
            LoadingOn(true);
        }

        // Event that fires when the QRCode image is clicked
        private async void QRCodeImage_Clicked(object sender, EventArgs e)
        {
            if (DisplayedQR != null)
            {
                LoadingOn(true);
                SaveFileDialog saveDialog = new SaveFileDialog();

                saveDialog.Filter = "image files (*.bmp)|*.bmp";
                saveDialog.FileName = DisplayedQR.QRType + DisplayedQR.TokenValue.ToString() + ".bmp";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    await azureHelper.BlobToFile(QRCodeContainer, DisplayedQR.RawData, saveDialog.FileName);
                    ClearQRPanelValue();
                }
                LoadingOn(false);
            }
        }

        // Uploads the QR code to the database
        private async void UploadQRCode(string path)
        {
            await azureHelper.PushFileBlob(QRCodeContainer, DisplayedQR.RawData, path);
            DownloadQRCodes();
        }

        // Event executed when delete QR button is clicked on the QR panel
        private void QRDelete_BTN_Click(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(QRTypeDropDown.Text))
            {
                DeleteQRCode();
                QRDelete_BTN.Enabled = false;
                ClearQRPanelValue();
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Deletes a QR from the qrcodes database
        private async void DeleteQRCode()
        {
            await azureHelper.DeleteBlob(QRCodeContainer, QRCodes[QRCodeList.SelectedIndex - 1].RawData);
            DownloadQRCodes();
            ClearQRPanelValue();
        }

        // Event executed when QR code selected in qr list
        private async void QRCodeListSelectedIndex_Changed(object sender, EventArgs e)
        {
            if (QRCodeList.SelectedIndex > 0)
            {
                if (QRCodeList.SelectedItem.ToString() == "<Add New>")
                {
                    ClearQRPanelValue();
                    QRDelete_BTN.Enabled = false;
                    QRSave_BTN.Enabled = false;
                }
                else
                {
                    DisplayedQR = QRCodes[QRCodeList.SelectedIndex - 1];
                    if (File.Exists(BmpPath))
                    {
                        File.Delete(BmpPath);
                    }
                    await azureHelper.BlobToFile(QRCodeContainer, DisplayedQR.RawData, BmpPath);

                    QRTypeDropDown.SelectedIndex = QREnumNames.FindIndex(DisplayedQR.QRType.ToString().Equals);
                    QRTokenValue.Text = DisplayedQR.TokenValue.ToString();
                    QRPicture.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(BmpPath)));

                    QRDelete_BTN.Enabled = true;
                    QRSave_BTN.Enabled = true;
                }
            }
        }

        // Clears the values on the QR panel
        private void ClearQRPanelValue()
        {
            QRTypeDropDown.SelectedIndex = -1;
            QRTokenValue.Text = "";
            QRPicture.Image = null;
        }

        // Event executed when the QR Token value changes
        private void QRTokenValue_TextChanged(object sender, EventArgs e)
        {
            if (QRTokenValue.Text.Length > 0)
            {
                Regex reg = new Regex(@"(\d+)");
                if (!reg.IsMatch(QRTokenValue.Text.Last().ToString()) || QRTokenValue.Text.Length > 10)
                {
                    List<char> list = QRTokenValue.Text.ToCharArray().ToList();
                    list.RemoveAt(list.Count - 1);
                    QRTokenValue.Text = new string(list.ToArray());
                    QRTokenValue.SelectionStart = QRTokenValue.Text.Length;

                    try
                    {
                        int.Parse(QRTokenValue.Text);
                    }
                    catch
                    {
                        QRTokenValue.Text = int.MaxValue.ToString();
                    }
                }
            }
        }

        // event occures when any key is pressed while qrcode list box is in focus
        private void QRCodeListBox_KeyPressed(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region Events
        /// <summary> Azure container name for the Events </summary>
        public string EventsContainer = "events"; // REQUIRED-FIELD : Container for the Calander Events can be created in azure portal or microsoft azure storage explorer
        /// <summary> States for the event panel </summary>
        public enum EventState { Idle, Adding, Updating }
        /// <summary> Current state of the event panel </summary>
        public EventState curEventState = EventState.Idle;
        /// <summary> List of events downloaded from the blobstore </summary>
        public List<CalendarEvent> Events;
        /// <summary> Current event selected on the event panel </summary>
        public CalendarEvent SelectedEvent;
        /// <summary> Current month displayed in the calendar </summary>
        private int monthInt;
        /// <summary> Allows for the calendar date selected event to fire only at the correct times </summary>
        private bool ManualPick = true;

        // Sets the state of the event panel
        private void SetEventState(EventState newState)
        {
            curEventState = newState;
            switch (newState)
            {
                case EventState.Idle:
                    ClearEventFields();
                    EventInputPanel.Visible = true;
                    AddEvent_BTN.Enabled = true;
                    SaveEvent_BTN.Enabled = false;
                    DeleteEvent_BTN.Enabled = false;
                    break;
                case EventState.Adding:
                    EventInputPanel.Visible = true;
                    AddEvent_BTN.Enabled = false;
                    SaveEvent_BTN.Enabled = true;
                    DeleteEvent_BTN.Enabled = false;
                    break;
                case EventState.Updating:
                    EventInputPanel.Visible = true;
                    AddEvent_BTN.Enabled = false;
                    SaveEvent_BTN.Enabled = true;
                    DeleteEvent_BTN.Enabled = true;
                    break;
            }
        }

        // Sets up the even inputs on event panel
        private void EventSetup()
        {
            ManualPick = false;
            monthInt = DateTime.Today.Month;

            StartDatePicker.MinDate = new DateTime(1970, 01, 01);
            StartDatePicker.MaxDate = new DateTime(2037, 01, 19);

            EndDatePicker.MinDate = new DateTime(1970, 01, 01);
            EndDatePicker.MaxDate = new DateTime(2037, 01, 19);
        }

        // Downloads the events of currently selected month from the blobstore
        private async void DownloadMonthsEvents()
        {
            LoadingOn(true);
            EventsList.Items.Clear();
            Events = new List<CalendarEvent>(await azureHelper.GetByPartitionKey(EventsContainer, CalendarEvent.DateToPartitionMonth(EventCalendar.SelectionStart)));

            EventsList.Items.Add(String.Format("~{0, -31} | {1, -12} | {2, -12} |\t", new string[] { "Name", "Start Date", "End Date" }));

            foreach (CalendarEvent calEvent in Events)
            {
                if (AdminUtilities.ViewClearenceCheck(calEvent.Org))
                {
                    EventsList.Items.Add(calEvent.ToString());
                }
            }
            LoadingOn(false);
        }

        // Event executed when the selected item in the event list changes
        private void EventListSelectedIndex_Changed(object sender, EventArgs e)
        {
            if (EventsList.SelectedIndex > 0)
            {
                ManualPick = false;
                SetEventState(EventState.Updating);
                SelectedEvent = Events[EventsList.SelectedIndex - 1];

                StartDatePicker.Value = SelectedEvent.GetStart();
                EndDatePicker.Value = SelectedEvent.GetEnd();

                EventNameTextBox.Text = SelectedEvent.Name;
                EventDetailsTextBox.Text = SelectedEvent.Details;
            }
        }

        // Event executed when the date selected on the event calendar changes
        private void EventCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (EventCalendar.SelectionStart.Month != monthInt)
            {
                monthInt = EventCalendar.SelectionStart.Month;
                DownloadMonthsEvents();
            }
            else
            {

            }

            if (curEventState != EventState.Idle)
            {
                SetEventState(EventState.Idle);
            }
        }

        // Event executed when the date of the start date picker changes
        private void StartDatePicker_DateChanged(object sender, EventArgs e)
        {
            if (ManualPick)
            {
                if (StartDatePicker.Value < DateTime.Now)
                {
                    StartDatePicker.Value = DateTime.Now;
                    ManualPick = false;
                }
                if (EndDatePicker.Value < StartDatePicker.Value)
                {
                    EndDatePicker.Value = StartDatePicker.Value;
                    ManualPick = false;
                }
            }
            ManualPick = true;
        }

        // Event executed when the date of the end date picker changesd
        private void EndDatePicker_DateChanged(object sender, EventArgs e)
        {
            if (ManualPick)
            {
                if (EndDatePicker.Value < StartDatePicker.Value)
                {
                    EndDatePicker.Value = StartDatePicker.Value;
                    MessageBox.Show("End Date must be after or on the Start Date", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            ManualPick = true;
        }

        // Event executed when the text in the event name input box changed
        private void EventNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventNameTextBox.Text.Length > 32)
            {
                List<char> list = EventNameTextBox.Text.ToCharArray().ToList();
                list.RemoveAt(list.Count - 1);
                EventNameTextBox.Text = new string(list.ToArray());
                EventNameTextBox.SelectionStart = EventNameTextBox.Text.Length;
            }
        }

        // Event executed when the add event button is clicked
        private void AddEventBTN_Clicked(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(EventOrgDropdown.Text))
            {
                SetEventState(EventState.Adding);
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Event executed when save event button clicked
        private async void SaveEventBTN_Clicked(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(EventOrgDropdown.Text))
            {
                if (EventNameTextBox.Text.Length > 0)
                {
                    if (curEventState == EventState.Adding)
                    {
                        CalendarEvent newEvent = new CalendarEvent(StartDatePicker.Value, EndDatePicker.Value, EventNameTextBox.Text, EventDetailsTextBox.Text, EventOrgDropdown.Text);
                        await azureHelper.AddEvent(EventsContainer, newEvent);
                        DownloadMonthsEvents();
                        SetEventState(EventState.Idle);
                    }
                    else if (curEventState == EventState.Updating)
                    {
                        CalendarEvent newEvent = new CalendarEvent(StartDatePicker.Value, EndDatePicker.Value, EventNameTextBox.Text, EventDetailsTextBox.Text, EventOrgDropdown.Text);
                        if (SelectedEvent != newEvent)
                        {
                            await azureHelper.ReplaceEvent(EventsContainer, SelectedEvent, newEvent);
                        }
                        DownloadMonthsEvents();
                        SetEventState(EventState.Idle);
                    }
                }
                else
                {
                    MessageBox.Show("Must populate Name field", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("You are not part of that orgamization", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Event executed when delete event button is clicked
        private async void DeleteEventBTN_Clicked(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(EventOrgDropdown.Text))
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this event.\n" + SelectedEvent.Name, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    await azureHelper.DeleteEvent(EventsContainer, SelectedEvent);
                }
                DownloadMonthsEvents();
                SetEventState(EventState.Idle);
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Clear the input fields and reset the dates on event panel
        private void ClearEventFields()
        {
            ManualPick = false;
            SelectedEvent = null;
            StartDatePicker.Value = DateTime.Today;
            EndDatePicker.Value = DateTime.Today;
            EventNameTextBox.Text = "";
            EventDetailsTextBox.Text = "";
        }

        // event occures when any key is pressed while Events list box is in focus
        private void EventsListBox_KeyPressed(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region Products
        /// <summary> Azure container name for the products </summary>
        public string ProductsContainer = "products"; // REQUIRED-FIELD : Container for the Products can be created in azure portal or microsoft azure storage explorer
        private List<Product> Products;
        /// <summary> States for the Product panel </summary>
        private enum ProductPanelStates { GENERATE, UPDATE }
        /// <summary> Current product panel state </summary>
        private ProductPanelStates CurProductState = ProductPanelStates.GENERATE;
        /// <summary> The currently selected product </summary>
        private Product DisplayedProduct;
        /// <summary> The value of the tokens for the cost estimation </summary>
        private float TokenValue = 0.001F;
        /// <summary> These are the multiplication tiers for the  </summary>
        private long[] TokenValueTiers = { 700, 1200, 2000, 5000 };
        /// <summary> The multiplier on the token value </summary>
        private float TokenValueMultiplier = 1.25f;
        /// <summary> Maximum character length for the value of the product </summary>
        private int MaxValueLength = 7;

        // Changes the state of the Product panel
        private async void ChangeProductState(ProductPanelStates newState)
        {
            CurProductState = newState;

            switch (CurProductState)
            {
                case ProductPanelStates.GENERATE:
                    ProductGenerateBTN.Text = "Generate";
                    ProductDeleteBTN.Visible = false;
                    ClearProductPanelValues();
                    break;
                case ProductPanelStates.UPDATE:
                    LoadingOn(true);
                    ProductGenerateBTN.Text = "Update";
                    ProductDeleteBTN.Visible = true;

                    ProductOrgDropdown.SelectedIndex = (int)DisplayedProduct.Organization;
                    ProductNameInput.Text = DisplayedProduct.ProductName;
                    ProductValueInput.Text = DisplayedProduct.ProductValue;
                    ProductDiscountInput.Text = DisplayedProduct.Discount;
                    ProductTokenValueInput.Text = DisplayedProduct.TokenValue.ToString();
                    if (ProductListBox.SelectedIndex > -1)
                    {
                        DisplayedProduct = Products[ProductListBox.SelectedIndex - 1];
                        if (File.Exists(BmpPath))
                        {
                            File.Delete(BmpPath);
                        }
                        await azureHelper.BlobToFile(ProductsContainer, DisplayedProduct.RawData, BmpPath);

                        ProductQRDisplay.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(BmpPath)));
                    }
                    break;
            }
            LoadingOn(false);
        }

        // Event executed when the generate button is pressed
        private async void ProductGenerate_BTN(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(ProductOrgDropdown.Text))
            {
                switch (CurProductState)
                {
                    case ProductPanelStates.GENERATE:
                        GenerateProduct();
                        await UploadProduct(BmpPath);
                        DownloadProducts();
                        break;
                    case ProductPanelStates.UPDATE:
                        await DeleteProduct(DisplayedProduct.RawData);
                        GenerateProduct();
                        await UploadProduct(BmpPath);
                        DownloadProducts();
                        ClearProductPanelValues();
                        break;
                }
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        /// <summary> Downloads the QR code data </summary>
        private async void DownloadProducts()
        {
            LoadingOn(true);
            ProductListBox.Items.Clear();
            Products = new List<Product>();

            CloudBlobContainer container = azureHelper.BlobClient.GetContainerReference(ProductsContainer);
            BlobContinuationToken blobConToken = null;
            BlobResultSegment result;

            do
            {
                result = await container.ListBlobsSegmentedAsync(blobConToken);
                blobConToken = result.ContinuationToken;
            } while (blobConToken != null);

            List<string> blobs = new List<string>();
            blobs.AddRange(result.Results.Cast<CloudBlockBlob>().Select(b => b.Name));
            foreach (string blob in blobs)
            {
                Product prod = Product.Parse(blob);
                if (AdminUtilities.ViewClearenceCheck(prod.Organization.ToString()))
                {
                    Products.Add(prod);
                }
            }

            Products = Products.OrderBy(o => o.Organization).ThenBy(o => o.Organization).ToList();

            ProductListBox.Items.Add(String.Format("~{0, -12} | {1, -32} | {2, -6} | {3, -3} | {4, -16} |\t", new string[] { "Organization", "Product Name", "$Value", "%Off", "Token Value" }));
            
            foreach (Product prod in Products)
            {
                ProductListBox.Items.Add(prod.ToString());
            }
            ProductListBox.Items.Add("<Add New>");
            LoadingOn(false);
        }

        // Event executes when the selected item in the products list changes
        private void ProductList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProductListBox.SelectedItem != null && ProductListBox.SelectedItem.ToString()[0] != '~')
            {
                if (ProductListBox.SelectedItem.ToString() != "<Add New>")
                {
                    DisplayedProduct = Products[ProductListBox.SelectedIndex - 1];
                    ChangeProductState(ProductPanelStates.UPDATE);
                }
                else
                {
                    DisplayedProduct = null;
                    ChangeProductState(ProductPanelStates.GENERATE);
                }
            }
            else
            {
                DisplayedProduct = null;
                ChangeProductState(ProductPanelStates.GENERATE);
            }
        }

        // Generates a product based on input on product panel
        private void GenerateProduct()
        {
            DisplayedProduct = new Product(ProductNameInput.Text, ProductValueInput.Text, ProductDiscountInput.Text, (Product.ProductOrg)Enum.Parse(typeof(Product.ProductOrg), ProductOrgDropdown.SelectedItem.ToString()), long.Parse(ProductTokenValueInput.Text));
            QRCodeData QRData = QRGenerator.CreateQrCode(DisplayedProduct.RawData, QRCodeGenerator.ECCLevel.Q);
            DisplayedProduct.Code = new QRCoder.QRCode(QRData);
            FileStream fs = new FileStream(BmpPath, FileMode.Create);
            DisplayedProduct.GetImage().Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            fs.Close();
        }

        // Uploads the Product to the database
        private async Task UploadProduct(string path)
        {
            LoadingOn(true);
            await azureHelper.PushFileBlob(ProductsContainer, DisplayedProduct.RawData, path);
            LoadingOn(false);
        }

        // Event that is executed when the image of the QR code is clicked
        private async void ProductQRImage_Clicked(object sender, EventArgs e)
        {
            if (DisplayedProduct != null)
            {
                LoadingOn(true);
                SaveFileDialog saveDialog = new SaveFileDialog();

                saveDialog.Filter = "image files (*.bmp)|*.bmp";
                saveDialog.FileName = DisplayedProduct.ProductName + ".bmp";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    await azureHelper.BlobToFile(ProductsContainer, DisplayedProduct.RawData, saveDialog.FileName);
                    ClearProductPanelValues();
                }
                LoadingOn(false);
            }
        }

        // event executes when text chages in the Product name input field. Caps length at 32
        private void ProductNameInput_TextChanged(object sender, EventArgs e)
        {
            if (ProductNameInput.Text.Length > 32)
            {
                List<char> list = ProductNameInput.Text.ToCharArray().ToList();
                list.RemoveAt(ProductNameInput.Text.Length - 1);
                ProductNameInput.Text = new string(list.ToArray());
                ProductNameInput.SelectionStart = ProductNameInput.Text.Length;
            }
        }

        // Event executed when the Delete product button is clicked
        private async void ProductDelete_BTN(object sender, EventArgs e)
        {
            if (AdminUtilities.ModificationClearenceCheck(ProductOrgDropdown.Text))
            {
                await DeleteProduct(DisplayedProduct.RawData);
                DownloadProducts();
                ChangeProductState(ProductPanelStates.GENERATE);
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        // Deletes a product with a matching raw data
        private async Task DeleteProduct(string productData)
        {
            LoadingOn(true);
            await azureHelper.DeleteBlob(ProductsContainer, productData);
            LoadingOn(false);
        }

        // Clears the input fields on the prodeuct panel
        private void ClearProductPanelValues()
        {
            ProductOrgDropdown.SelectedIndex = 0;
            ProductNameInput.Text = "";
            ProductValueInput.Text = "0.00";
            ProductDiscountInput.Text = "0";
            ProductTokenValueInput.Text = "0";
            ProductQRDisplay.Image = null;
        }

        // Event executed when the text in the Value input changes
        private void ProductValue_TextChanged(object sender, EventArgs e)
        {
            if (ProductValueInput.Text.Length > 0 && ProductValueInput.Text[ProductValueInput.Text.Length - 1] != '.')
            {
                Decimal value;
                if (Decimal.TryParse(ProductValueInput.Text, out value) && ProductValueInput.Text.Length < MaxValueLength)
                {
                    value = Math.Round(value, 2);
                    ProductValueInput.Text = value.ToString();
                    ProductValueInput.SelectionStart = ProductValueInput.Text.Length;
                }
                else
                {
                    List<char> list = ProductValueInput.Text.ToCharArray().ToList();
                    list.RemoveAt(ProductValueInput.Text.Length - 1);
                    ProductValueInput.Text = new string(list.ToArray());
                    ProductValueInput.SelectionStart = ProductValueInput.Text.Length;
                }
                CalculateTokens();
            }
        }

        // Event executed when the text in the Discount % input changes
        private void ProductDiscount_TextChanged(object sender, EventArgs e)
        {
            if (ProductDiscountInput.Text.Length > 0)
            {
                int discount;
                if (!int.TryParse(ProductDiscountInput.Text, out discount) || ProductDiscountInput.Text.Length > 3)
                {
                    List<char> list = ProductDiscountInput.Text.ToCharArray().ToList();
                    list.RemoveAt(ProductDiscountInput.Text.Length - 1);
                    ProductDiscountInput.Text = new string(list.ToArray());
                    ProductDiscountInput.SelectionStart = ProductDiscountInput.Text.Length;
                }
                else
                {
                    if (discount > 100)
                    {
                        discount = 100;
                        ProductDiscountInput.Text = discount.ToString();
                        ProductDiscountInput.SelectionStart = ProductDiscountInput.Text.Length;
                    }

                    if (ProductDiscountInput.Text.Length > 1 && ProductDiscountInput.Text[0] == '0')
                    {
                        List<char> list = ProductDiscountInput.Text.ToCharArray().ToList();
                        list.RemoveAt(0);
                        ProductDiscountInput.Text = new string(list.ToArray());
                        ProductDiscountInput.SelectionStart = ProductDiscountInput.Text.Length;
                    }
                }
                CalculateTokens();
            }
            else
            {
                ProductDiscountInput.Text = "0";
                ProductDiscountInput.SelectionStart = ProductDiscountInput.Text.Length;
            }
        }

        // Returns the proper Token value depending on the value tier
        private double GetValueTier(long cents)
        {
            for (int i = 0; i < TokenValueTiers.Length; ++i)
            {
                if (cents < TokenValueTiers[i])
                {
                    if (i != 0)
                    {
                        double temp = (double)TokenValue * Math.Pow(TokenValueMultiplier, i);
                        return temp;
                    }
                    break;
                }
                else if (cents > TokenValueTiers[i] && i == TokenValueTiers.Length - 1)
                {
                    double temp = (double)TokenValue * Math.Pow(TokenValueMultiplier, i + 1);
                    return temp;
                }
            }
            return TokenValue;
        }

        // Calculates the toekn value of a product and populates the display box
        private void CalculateTokens()
        {
            Decimal value;
            if (Decimal.TryParse(ProductValueInput.Text, out value))
            {
                if (value > 0)
                {
                    long cents = 0;
                    if (ProductValueInput.Text.Contains('.'))
                    {
                        string temp = ProductValueInput.Text;
                        List<char> list = temp.ToCharArray().ToList();
                        list.RemoveAt(list.IndexOf('.'));
                        temp = new string(list.ToArray());
                        cents = long.Parse(temp);
                    }
                    else
                    {
                        cents = long.Parse(ProductValueInput.Text) * 100;
                    }

                    float totaltokens = ((float)cents / (float)GetValueTier(cents));
                    long totalTokenValue = Convert.ToInt64(totaltokens);
                    float tokenAmountAD = totalTokenValue - ((totalTokenValue / 100) * float.Parse(ProductDiscountInput.Text));
                    long result = totalTokenValue - (long)tokenAmountAD;

                    ProductTokenValueInput.Text = result.ToString();
                }
            }
        }

        // event occures when any key is pressed while Product list box is in focus
        private void ProductListBox_KeyPressed(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Trivia
        /// <summary> Azure container name for the Locations/pins/POIS for rewards </summary>
        public string triviaContainer = "trivia"; // REQUIRED-FIELD : Container for the trivia can be created in azure portal or microsoft azure storage explorer
        public string triviaPartition = "trivia"; // REQUIRED-FIELD : General partition for the trivia table
        /// <summary> List of questions downloaded from blobstore </summary>
        private List<TriviaQuestion> Questions;
        /// <summary> Trivia question currently selected </summary>
        private TriviaQuestion SelectedQuestion;
        /// <summary> States of the TriviaTab </summary>
        public enum TriviaState { Idle, Adding, Updating }
        /// <summary> Current state of the event panel </summary>
        public TriviaState curTriviaState = TriviaState.Idle;

        // Sets the state of the Trivia panel
        private void SetTriviaState(TriviaState newState)
        {
            curTriviaState = newState;
            switch (newState)
            {
                case TriviaState.Idle:
                    ClearTriviaFields();
                    TriviaAdd_BTN.Enabled = true;
                    TriviaDelete_BTN.Enabled = false;
                    break;
                case TriviaState.Adding:
                    TriviaAdd_BTN.Enabled = true;
                    TriviaDelete_BTN.Enabled = false;
                    break;
                case TriviaState.Updating:
                    TriviaAdd_BTN.Enabled = false;
                    TriviaDelete_BTN.Enabled = true;
                    break;
            }
        }

        // Event executed when the add trivia button is clicked
        private async void AddQueationBTN_Clicked(object sender, EventArgs e)
        {
            LoadingOn(true);
            Button btn = (Button)sender;
            if(btn.Text == "Add")
            {
                if (AdminUtilities.ModificationClearenceCheck(EventOrgDropdown.Text))
                {
                    SetTriviaState(TriviaState.Adding);
                    TriviaQuestion newQuestion = new TriviaQuestion(triviaPartition, TriviaOrganization_Dropdown.Text + DateTime.Now.Ticks.ToString(), TriviaLocationBox.Text, TriviaQuestionBox.Text, TriviaAnswerABox.Text, TriviaAnswerBBox.Text, TriviaAnswerCBox.Text, TriviaAnswerDBox.Text, TriviaCorrectAnswerBox.Text, int.Parse(TriviaValueBox.Text));
                    await azureHelper.AddQuestion(triviaContainer, newQuestion);
                    DownloadTrivia();
                    SetTriviaState(TriviaState.Idle);
                }
                else
                {
                    MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else if(btn.Text == "Update")
            {
                TriviaQuestion newQuestion = new TriviaQuestion(triviaPartition, TriviaOrganization_Dropdown.Text + DateTime.Now.Ticks.ToString(), TriviaLocationBox.Text, TriviaQuestionBox.Text, TriviaAnswerABox.Text, TriviaAnswerBBox.Text, TriviaAnswerCBox.Text, TriviaAnswerDBox.Text, TriviaCorrectAnswerBox.Text, int.Parse(TriviaValueBox.Text));
                if (SelectedQuestion != newQuestion)
                {
                    await azureHelper.ReplaceQuestion(triviaContainer, SelectedQuestion, newQuestion);
                }
                DownloadTrivia();
                SetTriviaState(TriviaState.Idle);
            }
            ClearTriviaFields();
            LoadingOn(false);
        }

        // Event executed when delete trivia button is clicked
        private async void DeleteTriviaBTN_Clicked(object sender, EventArgs e)
        {
            LoadingOn(true);
            if (AdminUtilities.ModificationClearenceCheck(TriviaOrganization_Dropdown.Text))
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this trivia Question.\n", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    await azureHelper.DeleteQuestion(triviaContainer, SelectedQuestion);
                }
                DownloadTrivia();
                SetTriviaState(TriviaState.Idle);
            }
            else
            {
                MessageBox.Show("You do not have proper clearance for this feature.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            ClearTriviaFields();
            LoadingOn(false);
        }

        // Clear the input fields and reset the dates on trivia panel
        private void ClearTriviaFields()
        {
            TriviaLocationBox.Text = "";
            TriviaQuestionBox.Text = "";
            TriviaAnswerABox.Text = "";
            TriviaAnswerBBox.Text = "";
            TriviaAnswerCBox.Text = "";
            TriviaAnswerDBox.Text = "";
            TriviaCorrectAnswerBox.Text = "";
            TriviaValueBox.Text = "";
            TriviaAdd_BTN.Text = "Add";
            TriviaOrganization_Dropdown.SelectedIndex = -1;

        }

        // Downloads the trivia questions
        private async void DownloadTrivia()
        {
            LoadingOn(true);
            TriviaListBox.Items.Clear();
            Questions = new List<TriviaQuestion>(await azureHelper.GetByPartitionKeyTrivia(triviaContainer, triviaPartition));

            // Header Line
            TriviaListBox.Items.Add(String.Format("{0, -32} | {1, -32} | {2, -12} | {3, -16} | {4, -12} | {5, -12} | {6, -12} | {7, -12} | {8, -6} \t", new string[] { "Organization", "Location", "Qeustion", "Correct Answer", "Answer A", "Answer B", "Answer C", "Answer D", "Value" }));

            foreach (TriviaQuestion question in Questions)
            {
                if (AdminUtilities.ViewClearenceCheck(question.Org))
                {
                    TriviaListBox.Items.Add(question.ToString());
                }
            }

            TriviaListBox.Items.Add("<Add New>");
            LoadingOn(false);
        }

        // Nullifies keypresses when list selected
        private void TriviaList_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        // HAndles the index changing in the trivia lists
        private void TriviaList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TriviaListBox.SelectedIndex > 0)
            {
                var item = TriviaListBox.SelectedItem;
                if (item.ToString() == "<Add New>")
                {
                    SetTriviaState(TriviaState.Adding);
                    TriviaDelete_BTN.Enabled = false;
                    TriviaAdd_BTN.Text = "Add";

                    ClearTriviaFields();
                }
                else
                {
                    SetTriviaState(TriviaState.Updating);
                    TriviaDelete_BTN.Enabled = true;
                    TriviaAdd_BTN.Text = "Update";

                    SelectedQuestion = Questions[TriviaListBox.SelectedIndex - 1];

                    TriviaLocationBox.Text = SelectedQuestion.Location;
                    TriviaQuestionBox.Text = SelectedQuestion.Question;
                    TriviaAnswerABox.Text = SelectedQuestion.AnswerA;
                    TriviaAnswerBBox.Text = SelectedQuestion.AnswerB;
                    TriviaAnswerCBox.Text = SelectedQuestion.AnswerC;
                    TriviaAnswerDBox.Text = SelectedQuestion.AnswerD;
                    TriviaCorrectAnswerBox.Text = SelectedQuestion.CorrectAnswer;
                    TriviaValueBox.Text = SelectedQuestion.Value.ToString();

                    List<string> orgs = new List<string>(Enum.GetNames(typeof(AdminUtilities.Organization)).ToArray());

                    for (int i = 0; i < orgs.Count; ++i)
                    {
                        if(SelectedQuestion.Org.Contains(orgs[i].ToString()))
                        {
                            TriviaOrganization_Dropdown.SelectedIndex = i;
                        }
                    }
                }
            }
        }

        // Event executed when the QR Token value changes
        private void TriviaTokenValue_TextChanged(object sender, EventArgs e)
        {
            if (TriviaValueBox.Text.Length > 0)
            {
                Regex reg = new Regex(@"(\d+)");
                if (!reg.IsMatch(TriviaValueBox.Text.Last().ToString()) || TriviaValueBox.Text.Length > 10)
                {
                    List<char> list = TriviaValueBox.Text.ToCharArray().ToList();
                    list.RemoveAt(list.Count - 1);
                    TriviaValueBox.Text = new string(list.ToArray());
                    TriviaValueBox.SelectionStart = TriviaValueBox.Text.Length;

                    try
                    {
                        int.Parse(TriviaValueBox.Text);
                    }
                    catch
                    {
                        TriviaValueBox.Text = int.MaxValue.ToString();
                    }
                }
            }
        }
        #endregion
    }
}
