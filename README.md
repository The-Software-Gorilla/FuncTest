# Setup Instructions

To get this project working, we need to set up a few things:
1. **Update brew**: Make sure you have the latest version of Homebrew.
   ```bash
   brew update
   ```
2. **Upgrade brew**: Upgrade Homebrew to ensure all packages are up to date.
   ```bash
   brew upgrade
   ```
3. **Install/update NPM**: Install NPM so it can run azurite.
    ```bash
    brew install npm
    ```
    If you already have NPM installed, you can update it with:
    ```bash
    npm install -g npm
    ```
4. **Install the Azure Command Line Interface (CLI)**: This is necessary for managing Azure resources.
   ```bash
   brew install azure-cli
   ```
5. **Install Azurite**: Azurite is the Azure Storage emulator that allows you to run Azure Storage locally.
   ```bash
   npm install -g azurite
   ```
6. **Verify you can get at both Azurite and Azure CLI**: Check if both tools are installed correctly.
   ```bash
   az --version
   azurite --version
   ```
7. **Install mkcert**: This tool is used to create local SSL certificates.
   ```bash
   brew install mkcert
   ```
8. **Install nss if you're using FireFox**: This is required for mkcert to work with Firefox if you're using it.
   ```bash
   brew install nss
    ```
9. **Create a local certificate**: This step is necessary for running the application with HTTPS.
    ```bash
    mkcert -install
   ```
10. **Get the location of the certificate**: This is where the generated certificate will be stored.
    ```bash
    mkcert -CAROOT
    ```
11. **Create a certificate for localhost**: This will generate a certificate for local development.
    ```bash
    mkcert localhost
    ```
12. **Move the certificate to mkcert directory**: Move the generated certificate to the mkcert directory.
    ```bash
    mv localhost.pem $(mkcert -CAROOT)/localhost.pem
    mv localhost-key.pem $(mkcert -CAROOT)/localhost-key.pem
    ```
13. **Create a certificate for 127.0.0.1**: This is necessary for local development on the loopback address.
    ```bash
    mkcert 127.0.0.1
    ```
14. **Move the certificate to mkcert directory**: Move the generated certificate for
    ```bash
    mv 127.0.0.1.pem $(mkcert -CAROOT)/127.0.0.1.pem
    mv 127.0.0.1-key.pem $(mkcert -CAROOT)/127.0.0.1-key.pem
    ```
15. **Create a working directory to run Azurite**: This is where Azurite will store its data.
    ```bash
    mkdir -p ~/Workspaces/azurite/
    ```
16. **Create a script to run Azurite**: Create a script that will start Azurite with the necessary parameters.
    ```bash
    vi ~/Workspaces/start-azurite.sh
    i
    azurite --silent --location /Users/brucegruenbaum/Workspaces/azurite --debug /Users/brucegruenbaum/Workspaces/azurite/debug.log --cert /Users/brucegruenbaum/Library/'Application Support'/mkcert/127.0.0.1.pem --key /Users/brucegruenbaum/Library/'Application Support'/mkcert/127.0.0.1-key.pem --oauth basic --loose
    :wq
    ```
17. **Make the script executable**: This allows you to run the script to start Azurite.
    ```bash
    chmod +x ~/Workspaces/azurite/start-azurite.sh
    ```
18. **Run the script to start Azurite**: This will start the Azurite emulator.
    ```bash
    ~/Workspaces/azurite/start-azurite.sh
    ```
19. **Install the Azure Storage Explorer**: This is a GUI tool to manage Azure Storage resources. The instructions for setting it up are available [here](https://medium.com/@nischay_sharma/step-by-step-guide-on-how-to-set-up-azurite-locally-102332fc2ec). Check the section "Using Azurite with Storage Explorer" for details on how to connect it to your local Azurite instance.
20. **Install the Azure Functions Core Tools**: This is necessary for running Azure Functions locally.
    ```bash
    brew tap azure/functions
    brew install azure-functions-core-tools@4
    ```
21. **Verify the Azure Functions Core Tools installation**: Check if the tools are installed correctly.
    ```bash
    func --version
    ```
22. **Make sure Azure Functions Template is installed**: This is necessary for creating and managing Azure Functions.
    ```bash
    dotnet new install Microsoft.Azure.Functions.Worker.ProjectTemplates
    ```
23. **Create a new Azure Functions project**: This will set up a new Azure Functions project in the current directory.
    ```bash
    func init <folder> --worker-runtime dotnet-isolated
    ```
24. **Make sure Azure Worker Item Templates are installed**: This is necessary for creating Azure Functions within the project.
    ```bash
    dotnet new install Microsoft.Azure.Functions.Worker.ItemTemplates
    ```
25. **Create a new Azure Functions function**: This will create a new function within the Azure Functions project.
    ```bash
    func new --name <function-name> --template "Http Trigger" --authlevel anonymous
    ```