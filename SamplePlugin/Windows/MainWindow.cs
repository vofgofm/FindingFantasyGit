using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;



namespace FindingFantasy.Windows;

#region otherclasses
public class SignInResponse
{
    public int StatusCode { get; set; }
    public string Body { get; set; }
}

public class TokenResponse
{
    [JsonProperty("AccessToken")]
    public string AccessToken { get; set; }

    // Include other fields if needed
}

public class LambdaResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("attributes")]
    public UserAttributes Attributes { get; set; }
}


public class UserAttributesResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("attributes")]
    public UserAttributes Attributes { get; set; }
}

public class UserAttributes
{
    [JsonProperty("sub")]
    public string Sub { get; set; }

    [JsonProperty("custom:InGameLastName")]
    public string InGameLastName { get; set; }

    [JsonProperty("custom:HomeServer")]
    public string HomeServer { get; set; }

    [JsonProperty("custom:DiscordName")]
    public string DiscordName { get; set; }

    [JsonProperty("custom:InGameFirstName")]
    public string InGameFirstName { get; set; }

    [JsonProperty("custom:AboutMe")]
    public string AboutMe { get; set; }

    [JsonProperty("custom:MareCode")]
    public string MareCode { get; set; }
}
#endregion


public class MainWindow : Window, IDisposable
{
    private IDalamudTextureWrap LogoImage;
    private Plugin Plugin;
    private object ClientState;
    private bool isAuthenticated = false; // Tracks authentication status
    private string email = ""; // User input for email
    private string password = ""; // User input for password
    private bool isSigningIn = false; // Tracks if a sign-in operation is in progress
    private string signInErrorMessage = ""; // Stores any sign-in error message
    private bool isCreatingAccount = false;
    private string username = "";
    private string firstName = "";
    private string lastName = "";
    private string server = "";
    private string mareCode = ""; // Optional
    private string discordName = ""; // Optional
    private string aboutMe = "";
    private string validationErrorMessage = ""; // To store validation error message
    private bool isCreatingAccountProcess = false; // Tracks if an account creation operation is in progress
    private bool profileInfoRetrieved = false; // Tracks if the user's profile info has been retrieved
    private string profileImagePath = ""; // Local path for the profile image
    private string uploadImagePath = ""; // Path entered by the user for image upload
    private string accessToken = ""; // Add this line to define accessToken
    private string uploadErrorMessage = ""; // Stores any upload error message
    // Boolean to control whether we are displaying the user's own profile
    private bool isDrawingOwnProfile = false;

    // Class-level variables to hold user profile information
    private string displayFirstName = "Loading...";
    private string displayLastName = "Loading...";
    private string displayServer = "Loading...";
    private string displayMareCode = "Loading...";
    private string displayDiscordName = "Loading...";
    private string displayAboutMe = "Loading...";

    // Class-level variable to hold the profile image path
    private string profileImageLocalPath = @"c:\findingfantasy\profile.png";

    private bool profileUpdateSuccess = false;
    private string profileUpdateMessage = "";
    private string downloadErrorMessage = "";
    private IDalamudTextureWrap profileImage;
    private DalamudPluginInterface pluginInterface;







    public MainWindow(Plugin plugin, IDalamudTextureWrap logoImage, DalamudPluginInterface PluginInterface) : base(
        "Finding Fantasy", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
           MinimumSize = new Vector2(800, 450), //change later
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue) //change later
        };

        this.LogoImage = logoImage;
        this.Plugin = plugin;
        this.pluginInterface = PluginInterface;

    }

    public void Dispose()
    {
        if (profileImage != null)
        {
            profileImage.Dispose();
            profileImage = null;
        }
    }

    public override void Draw()
    {
        if (isSigningIn)
        {
            ImGui.SetCursorPos(new Vector2(450, 100));
            ImGui.Text("Signing in...");
        }
        else if (!string.IsNullOrEmpty(signInErrorMessage))
        {
            ImGui.SetCursorPos(new Vector2(450, 100));
            ImGui.Text(signInErrorMessage);
        }

        if (isAuthenticated)
        {
            // logic for authenticated users
            

            if (ImGui.BeginTabBar("TabBar"))
            {
                if (ImGui.BeginTabItem("Profile"))
                {
                    //ImGui.Text("Welcome to Finding Fantasy!");
                    // Retrieve and display profile information
                    if (!profileInfoRetrieved)
                    {
                        RetrieveUserProfile();
                        DownloadProfileImage();
                        profileInfoRetrieved = true; // Set to true to avoid repeated calls
                    }
                    else
                    {
                        // Display profile information
                        DisplayUserProfile();
                        DrawProfileImage();
                    }

                    // Display profile image upload section
                    DisplayImageUploadSection();

                    if (!string.IsNullOrEmpty(downloadErrorMessage))
                    {
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), downloadErrorMessage);
                    }


                    // Update profile button


                    ImGui.SameLine();


                    if (!string.IsNullOrEmpty(profileUpdateMessage))
                    {
                        var messageColor = profileUpdateSuccess ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1); // Green if success, red otherwise
                        ImGui.TextColored(messageColor, profileUpdateMessage);
                    }


                    ImGui.SameLine();

                    if (!string.IsNullOrEmpty(uploadErrorMessage))
                    {
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), uploadErrorMessage); // Display upload error message in red
                    }

                    //fetchanddisplayprofile
                    
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Swiping"))
                {
                    // Content for "Find Love" tab
                    ImGui.Text("Find your match here!");

                    // Implement logic for finding love
                    // ...

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Matches"))
                {
                    // Content for "Matches" tab
                    ImGui.Text("Your matches will be displayed here.");

                    // Implement logic to display matches
                    // ...

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        else
        {
            DrawLoginOrCreateAccountUI();
        }
    }

    #region Update And Display Profile
    

  
  




    private async System.Threading.Tasks.Task<(string downloadUrl, string errorMessage)> GetPreSignedDownloadUrl()
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            string lambdaApiUrl = "https://5duykxcinj.execute-api.us-east-2.amazonaws.com/DownloadProfileImageStage/download";

            try
            {
                var response = await httpClient.GetAsync(lambdaApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(content);
                    return (data.downloadUrl, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (null, $"Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Exception occurred: {ex.Message}");
            }
        }
    }


    private async System.Threading.Tasks.Task DownloadImage(string preSignedUrl)
    {
        string directoryPath = @"c:\findingfantasy"; // Directory where the file will be saved
        string fileName = Path.GetFileName(new Uri(preSignedUrl).LocalPath); // Extracts the file name from the URL
        string localFilePath = Path.Combine(directoryPath, fileName); // Combines the directory path with the file name

        using (var httpClient = new HttpClient())
        {
            try
            {
                var response = await httpClient.GetAsync(preSignedUrl);
                if (response.IsSuccessStatusCode)
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(directoryPath); // This will create the directory if it doesn't exist

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                    Console.WriteLine($"Image downloaded successfully to {localFilePath}.");
                }
                else
                {
                    Console.WriteLine($"Failed to download image. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during image download: {ex.Message}");
            }
        }
    }



    private async System.Threading.Tasks.Task DownloadProfileImage()
    {
        

        var (preSignedDownloadUrl, errorMessage) = await GetPreSignedDownloadUrl();
        if (!string.IsNullOrEmpty(preSignedDownloadUrl))
        {
            await DownloadImage(preSignedDownloadUrl);
            downloadErrorMessage = "";

            string fileName = Path.GetFileName(new Uri(preSignedDownloadUrl).LocalPath);
            string directoryPath = @"c:\findingfantasy";
            string fullPath = Path.Combine(directoryPath, fileName);

            // Call LoadProfileImage here after ensuring the image is downloaded
            LoadProfileImage(fullPath);
        }
        else
        {
            downloadErrorMessage = errorMessage ?? "Failed to get the pre-signed download URL.";
        }
    }







    private void LoadProfileImage(string imagePath)
    {
        downloadErrorMessage = "";

        Console.WriteLine($"Attempting to load profile image from: {imagePath}");

        if (!File.Exists(imagePath))
        {
            downloadErrorMessage = "No Profile Image, upload one or your profile will not be visible to others";
            Console.WriteLine(downloadErrorMessage);
            return;
        }

        try
        {
            profileImage?.Dispose();
            profileImage = this.pluginInterface.UiBuilder.LoadImage(imagePath);

            if (profileImage != null)
            {
                Console.WriteLine("Image loaded successfully.");
            }
            else
            {
                downloadErrorMessage = "Failed to load image: Image is null after loading.";
                Console.WriteLine(downloadErrorMessage);
            }
        }
        catch (Exception ex)
        {
            downloadErrorMessage = $"Exception during image loading: {ex.Message}";
            Console.WriteLine(downloadErrorMessage);
        }
    }



    private void DrawProfileImage()
    {


        if (profileImage != null)
        {
            // Set the size for the image
            ImGui.SetCursorPos(new Vector2(0, 75));
            var imageSize = new Vector2(300, 300);

            // Draw the image
            ImGui.Image(profileImage.ImGuiHandle, imageSize);
        }
        else
        {
            // Draw placeholder text or image if the profile image is not loaded
            //ImGui.Text("Profile image not loaded.");
        }
    }


    private async System.Threading.Tasks.Task UpdateUserProfile()
    {

        profileUpdateMessage = "";

        if (!isAuthenticated || string.IsNullOrEmpty(accessToken))
        {
            profileUpdateMessage = "User is not authenticated or accessToken is missing.";
            profileUpdateSuccess = false;
            return;
        }



        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var updateProfileData = new
        {
            InGameFirstName = displayFirstName,
            InGameLastName = displayLastName,
            HomeServer = displayServer,
            MareCode = displayMareCode,
            DiscordName = displayDiscordName,
            AboutMe = displayAboutMe
        };

        var jsonContent = JsonConvert.SerializeObject(updateProfileData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync("https://smhsc55zyd.execute-api.us-east-2.amazonaws.com/UpdateProfileStage/update", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                profileUpdateMessage = "Profile updated successfully.";
                profileUpdateSuccess = true;
            }
            else
            {
                profileUpdateMessage = $"Failed to update profile: {response.StatusCode} - {responseString}";
                profileUpdateSuccess = false;
            }
        }
        catch (Exception ex)
        {
            profileUpdateMessage = $"Exception occurred while updating profile: {ex.Message}";
            profileUpdateSuccess = false;
        }
    }


    #endregion


    private void DisplayImageUploadSection()
    {
        ImGui.SetCursorPos(new Vector2(25, 400));
        ImGui.SetNextItemWidth(100.0f);
        ImGui.InputText("Profile Image File Path", ref profileImagePath, 255);
        ImGui.SameLine();
        if (ImGui.Button("Upload Image"))
        {
            UploadProfileImage();
        }
    }



    #region Upload Image

    private async System.Threading.Tasks.Task UploadProfileImage()
    {
        uploadErrorMessage = ""; // Reset the error message each time

        // Validate the file path and image format
        if (!File.Exists(profileImagePath))
        {
            uploadErrorMessage = "File does not exist.";
            Console.WriteLine(uploadErrorMessage); // Log the error
            return;
        }
        if (!IsValidImageFormat(profileImagePath))
        {
            uploadErrorMessage = "Invalid image format.";
            Console.WriteLine(uploadErrorMessage); // Log the error
            return;
        }

        // Log that we are attempting to get the pre-signed URL
        Console.WriteLine("Attempting to obtain pre-signed URL...");

        var preSignedUrl = await GetPreSignedUrl();
        if (string.IsNullOrEmpty(preSignedUrl))
        {
            // The error message will be logged in the GetPreSignedUrl method
            Console.WriteLine(uploadErrorMessage); // Log the error
            return;
        }

        // Log successful retrieval of the URL
        Console.WriteLine("Pre-signed URL obtained: " + preSignedUrl);

        // Log that we are starting the upload process
        Console.WriteLine("Starting image upload to S3...");

        var uploadSuccess = await UploadImageToS3(preSignedUrl, profileImagePath);
        if (!uploadSuccess)
        {
            // uploadErrorMessage will be set in the UploadImageToS3 method
            Console.WriteLine(uploadErrorMessage); // Log the error
            return;
        }

        uploadErrorMessage = "Image uploaded successfully.";
        Console.WriteLine(uploadErrorMessage); // Log success
        profileImage?.Dispose();
        await DownloadProfileImage();

        var imageUrl = new Uri(preSignedUrl).GetLeftPart(UriPartial.Path);
        UpdateUserProfileWithImageUrl(imageUrl);
    }



    private async System.Threading.Tasks.Task<string> GetPreSignedUrl()
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                uploadErrorMessage = "Sending request to API...";

                var response = await httpClient.PostAsync(
                    "https://34kx5vegv0.execute-api.us-east-2.amazonaws.com/uploadimagestage/upload",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Received response: " + responseBody);
                    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    return responseJson.uploadUrl;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var errorResponseJson = JsonConvert.DeserializeObject<dynamic>(errorBody);

                    // Handle unauthorized error specifically
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        uploadErrorMessage = "Unauthorized: Invalid or expired token.";
                    }
                    // Handle other potential errors
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        uploadErrorMessage = "Bad Request: The request was not understood by the server.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        uploadErrorMessage = "Forbidden: Access is denied.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        uploadErrorMessage = "Internal Server Error: The server encountered an unexpected condition.";
                    }
                    else
                    {
                        // Generic error message for other status codes
                        uploadErrorMessage = $"Error getting pre-signed URL: HTTP {response.StatusCode} - {errorBody}";
                    }

                    Console.WriteLine(uploadErrorMessage);
                    return null;
                }
            }
        }
        catch (HttpRequestException e)
        {
            uploadErrorMessage = $"Error sending request to API: {e.Message}";
            Console.WriteLine(uploadErrorMessage);
            return null;
        }
        catch (Exception e)
        {
            uploadErrorMessage = $"An error occurred: {e.Message}";
            Console.WriteLine(uploadErrorMessage);
            return null;
        }
    }






    private async System.Threading.Tasks.Task<bool> UploadImageToS3(string preSignedUrl, string filePath)
    {
        using (var httpClient = new HttpClient())
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var content = new StreamContent(fileStream))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            try
            {
                var response = await httpClient.PutAsync(preSignedUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Log detailed error information
                    var responseContent = await response.Content.ReadAsStringAsync();
                    uploadErrorMessage = $"Failed to upload image. Status Code: {response.StatusCode}. Response: {responseContent}";
                    Console.WriteLine(uploadErrorMessage);
                    return false;
                }
            }
            catch (Exception e)
            {
                uploadErrorMessage = $"Exception during image upload: {e.Message}";
                Console.WriteLine(uploadErrorMessage);
                return false;
            }
        }
    }

    private bool IsValidImageFormat(string filePath)
    {
        // Implement your image format validation logic here
        // For example, check the file extension
        var validFormats = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        return validFormats.Contains(fileExtension);
    }

    private void UpdateUserProfileWithImageUrl(string imageUrl)
    {
        // Implement the logic to update the user's profile with the new image URL
        // This could involve sending an update to your backend API
    }
    #endregion





    private async System.Threading.Tasks.Task RetrieveUserProfile()
    {
        if (!isAuthenticated || string.IsNullOrEmpty(accessToken))
        {
            displayAboutMe = "User is not authenticated or accessToken is missing.";
            return;
        }
        
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        string profileApiUrl = "https://97xy48sdkh.execute-api.us-east-2.amazonaws.com/FetchProfileStage/fetch";

        try
        {
            var response = await httpClient.GetAsync(profileApiUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var lambdaResponse = JsonConvert.DeserializeObject<LambdaResponse>(content);

                if (lambdaResponse.Message == "User verified")
                {
                    UpdateDisplayProfile(lambdaResponse.Attributes);
                    //displayAboutMe = "Profile successfully retrieved.";
                }
                else
                {
                    displayAboutMe = "Lambda response message indicates an error.";
                }
            }
            else
            {
                displayAboutMe = $"HTTP request failed: {response.StatusCode}, Content: {content}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            displayAboutMe = "HTTP Request Exception: " + httpEx.Message;
        }
        catch (Exception ex)
        {
            displayAboutMe = "General Exception: " + ex.Message;
        }
    }

    private void UpdateDisplayProfile(UserAttributes attributes)
    {
        displayFirstName = attributes.InGameFirstName ?? "Loading...";
        displayLastName = attributes.InGameLastName ?? "Loading...";
        displayServer = attributes.HomeServer ?? "Loading...";
        displayMareCode = attributes.MareCode ?? "Loading...";
        displayDiscordName = attributes.DiscordName ?? "Loading...";
        displayAboutMe = attributes.AboutMe ?? "Loading...";
    }







    private void DisplayUserProfile()
    {
        ImGui.SetCursorPos(new Vector2(450, 75));
        ImGui.SetNextItemWidth(200.0f);
        ImGui.InputText("In Game First Name", ref displayFirstName, 100);

        ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + 2));
        ImGui.SetNextItemWidth(200.0f);
        ImGui.InputText("In Game Last Name", ref displayLastName, 100);

        ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + 2));
        ImGui.SetNextItemWidth(200.0f);
        ImGui.InputText("Home Server", ref displayServer, 100);

        ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + 2));
        ImGui.SetNextItemWidth(200.0f);
        ImGui.InputText("Mare Code", ref displayMareCode, 100);

        ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + 2));
        ImGui.SetNextItemWidth(200.0f);
        ImGui.InputText("Discord Name", ref displayDiscordName, 100);

        // Calculate the position for the "About Me" input field
        Vector2 startPosition = new Vector2(450, ImGui.GetCursorPosY() + 2);

        // Calculate the available width based on the position and width of other input fields
        float availableWidth = 450 + 325 - startPosition.X;

        // Manually handle word wrapping for the "About Me" input field
        float textHeight = ImGui.GetTextLineHeightWithSpacing() * 7; // Adjust the height as needed
        ImGui.SetCursorPos(startPosition);
        ImGui.BeginChild("AboutMeChild", new Vector2(availableWidth, textHeight), true);

        // InputTextMultiline with word wrapping
        ImGui.InputTextMultiline("##AboutMeInput", ref displayAboutMe, 500, new Vector2(availableWidth, textHeight), ImGuiInputTextFlags.None);

        ImGui.EndChild();
        ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + 2));
        if (ImGui.Button("Update Profile"))
        {
            System.Threading.Tasks.Task.Run(async () => await UpdateUserProfile());
        }

        
    }







    #region SignIn
    private void DrawLoginOrCreateAccountUI()
    {


        // Login/Create UI elements
        if (isCreatingAccount)
        {
            DrawCreateAccountUI();
        }
        else
        {

            // Calculate the center position for the image
            // float centerY = (700 - 400) / 2; // (Window Height - Image Height) / 2

            // Define the size for the image and sections
            // Define the size for the image
            var imageSize = new Vector2(400, 400);

            // Draw the image on the left
            if (LogoImage != null)
            {
                // Image should be at the top-left corner
                ImGui.SetCursorPos(new Vector2(0, 0));
                ImGui.Image(LogoImage.ImGuiHandle, imageSize);
            }

            // Set initial cursor position for the first element
            ImGui.SetCursorPos(new Vector2(450, 125));

            // Input for username
            ImGui.SetNextItemWidth(200.0f);
            ImGui.InputText("Username", ref email, 25);

            // Get the size of the last item (in this case, the username input)
            Vector2 itemSize = ImGui.GetItemRectSize();

            // Use the height of the last item to offset the next item. 
            // You might want to add a little extra space, hence the '+ 8' or any other number for padding
            ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + itemSize.Y + 2));
            ImGui.SetNextItemWidth(200.0f);
            // Input for password. The cursor Y position is already set by the line above.
            ImGui.InputText("Password", ref password, 25, ImGuiInputTextFlags.Password);

            // Get the size of the last item (in this case, the username input)
            itemSize = ImGui.GetItemRectSize();

            // Use the height of the last item to offset the next item. 
            // You might want to add a little extra space, hence the '+ 8' or any other number for padding
            ImGui.SetCursorPos(new Vector2(450, ImGui.GetCursorPosY() + itemSize.Y + 2));


            if (ImGui.Button("Login"))
            {
                SignInUser(email, password);
            }

            ImGui.SameLine();

            if (ImGui.Button("Create Account"))
            {
                isCreatingAccount = true;
            }

            // Text position needs to be dynamic based on the window size
            // Tip Text - Below the image, aligned with the bottom of the login form
            ImGui.SetCursorPos(new Vector2(25, 415));
            ImGui.TextWrapped("Want more features added? Donate at Kofi.com/bigbard");

            // Chateau Text - Below the login form, aligned with the bottom of the window
            ImGui.SetCursorPos(new Vector2(450, 325));
            ImGui.SetNextItemWidth(315.0f);
            ImGui.TextWrapped("Need somewhere to take your date? The Chateau is open Thursdays and Saturdays 8:00pm-12:00am EST.\nPrimal - Lamia - Shirogane - W16 - P31 \n discord.gg/thechateauxiv");


        }
    }



    //refactor sign in user to check Lambda and Cognito
    private async System.Threading.Tasks.Task SignInUser(string username, string password)
    {
        isSigningIn = true;
        signInErrorMessage = "";

        var httpClient = new HttpClient();

        // Create a nested JSON object
        var innerData = new
        {
            username = username,
            password = password
        };
        var requestData = new
        {
            body = JsonConvert.SerializeObject(innerData)
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync("https://zq308lvjp0.execute-api.us-east-2.amazonaws.com/SignINStage/signin", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var signInResponse = JsonConvert.DeserializeObject<SignInResponse>(responseString);
                if (signInResponse != null && !string.IsNullOrEmpty(signInResponse.Body))
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(signInResponse.Body);
                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        isAuthenticated = true;
                        accessToken = tokenResponse.AccessToken; // Store the token
                                                                 // Additional logic for successful authentication
                    }
                    else
                    {
                        isAuthenticated = false;
                        signInErrorMessage = "Invalid credentials or authentication failed.";
                    }
                }
                else
                {
                    isAuthenticated = false;
                    signInErrorMessage = "Invalid credentials or authentication failed.";
                }
            }
            else
            {
                isAuthenticated = false;
                signInErrorMessage = "Invalid credentials or authentication failed.";
            }
        }
        catch (Exception ex)
        {
            isAuthenticated = false;
            signInErrorMessage = "Exception occurred: " + ex.Message;
        }
        finally
        {
            isSigningIn = false;
        }
    }





    private void DrawCreateAccountUI()
    {
        // Disclaimer text
        ImGui.TextWrapped("Please make your username and password unique, do NOT use your Final Fantasy / Square Enix username or password. By creating an account you agree you are over 21 and that developers of Finding Fantasy are not responsible for lost or stolen info");

        ImGui.Spacing(); // Add some space after the disclaimer

        ImGui.InputText("Username", ref username, 100);
        ImGui.InputText("Password", ref password, 100, ImGuiInputTextFlags.Password);
        ImGui.InputText("In Game First Name", ref firstName, 100);
        ImGui.InputText("In Game Last Name", ref lastName, 100);
        ImGui.InputText("Home Server (Example: Primal Lamia)", ref server, 100);
        ImGui.InputText("Mare Code (Optional)", ref mareCode, 100);
        ImGui.InputText("Discord Name (Optional)", ref discordName, 100);
        ImGui.Text("About Me:");

        // Calculate the available width based on the position and width of other input fields
        float availableWidth = 500;

        // Manually handle word wrapping for the "About Me" input field
        float textHeight = ImGui.GetTextLineHeightWithSpacing() * 5; // Adjust the height as needed
        //ImGui.SetCursorPos(startPosition);
        ImGui.BeginChild("AboutMeChild", new Vector2(availableWidth, textHeight), true);

        // InputTextMultiline with word wrapping
        ImGui.InputTextMultiline("##AboutMeInput", ref aboutMe, 500, new Vector2(availableWidth, textHeight), ImGuiInputTextFlags.None);

        ImGui.EndChild();

        //draw user image


        if (isCreatingAccountProcess)
        {
            ImGui.Text("Creating account...");
            if (ImGui.Button("Back"))
            {
                isCreatingAccountProcess = false;
                isCreatingAccount = true; // Go back to account creation form
            }
        }
        else if (!string.IsNullOrEmpty(validationErrorMessage))
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), validationErrorMessage); // Display validation error message in red
        }


        if (ImGui.Button("Create"))
        {
            if (ValidateAccountCreationFields())
            {
                CreateAccount();
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Back"))
        {
            isCreatingAccount = false;
        }

        if (!string.IsNullOrEmpty(validationErrorMessage))
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), validationErrorMessage); // Display validation error message in red
        }
    }

    //refactor create account to use Lambda and COgnito
    private void CreateAccount()
    {
        isCreatingAccountProcess = true;
        signInErrorMessage = "";

        System.Threading.Tasks.Task.Run(async () =>
        {
            var httpClient = new HttpClient();

            // Construct the inner JSON object
            var innerData = new
            {
                username = username,
                password = password,
                aboutMe = aboutMe,
                discordName = discordName,
                homeServer = server,
                inGameFirstName = firstName,
                inGameLastName = lastName,
                mareCode = mareCode
            };

            // Wrap the inner JSON object in a 'body' key
            var requestData = new
            {
                body = JsonConvert.SerializeObject(innerData)
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("https://4vg9nuqgu4.execute-api.us-east-2.amazonaws.com/FindingFantasyAPiStage/Register", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && responseString.Contains("User registration successful"))
                {
                    // Automatically sign in the user with the newly created account
                    await SignInUser(username, password);
                }
                else
                {
                    validationErrorMessage = "Error creating account: " + responseString;
                }
            }
            catch (Exception ex)
            {
                validationErrorMessage = "Exception occurred: " + ex.Message;
            }
            finally
            {
                isCreatingAccountProcess = false;
            }
        });
    }






    private bool ValidateAccountCreationFields()
    {
        // Reset validation error message
        validationErrorMessage = "";

        // Check if required fields are filled
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(server))

        {
            validationErrorMessage = "Please fill in all required fields.";
            return false;
        }



        // Additional validation can be added here (e.g., check file path validity)

        return true;
    }


    #endregion



}
