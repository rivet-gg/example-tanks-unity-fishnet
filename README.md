# Rivet + Unity + FishNet Example

This is a **work in progress** example of how to use Rivet with Unity and FishNet.

This is configured to use UDP sockets with the [Tugboat](https://fish-networking.gitbook.io/docs/manual/components/transports/tugboat) transport. WebGL requires the [Bayou](https://fish-networking.gitbook.io/docs/manual/components/transports/bayou) transport, which is not included in this demo yet.

## Prerequisites

- Unity 2020.3 + Linux Dedicated Server build module
- [Rivet CLI](https://github.com/rivet-gg/cli)

## Getting started

### Testing Locally

Rivet's APIs are all available to test locally with fake data. This is useful for testing your game without having to
deploy to Rivet.

1. Run `rivet init`
   - You will need to create a new game if you haven't already
   - Select `unity` engine type
   - Once complete, copy the token that looks like `dev_staging.XXXX` to your clipboard
2. Paste this token under the API token
   ![Update token](./docs/media/update_token.png)
3. Click play in the editor and click _Find Lobby_
   - You should see a lobby ID of `00000000-0000-0000-0000-000000000000` in the top left. This means you're testing against your local machine.
   - If this causes an error, you may need to navigate to _File > Build Settings_ and switch to the _Windows, Mac, Linux_ platform
   ![Running locally](./docs/media/running_locally.png)

### Deploying to Rivet

**Deploying the server**

1. Click _File > Build Settings_
2. Select _Dedicated Server_ from the list of platforms
   - Click the _Switch Platform_ button if it's not selected already
   - Validate _Target Platform_ is set to _Linux_
   ![Build settings](./docs/media/build_settings.png)
3. Click _Build_
4. Create a folder named `build/LinuxServer` and save with the name `LinuxServer`. Click _Save_.
   - Validate that you see a file located at `build/LinuxServer/LinuxServer.x86_64`
   ![Build folder](./docs/media/build_folder.png)
5. Run `rivet deploy -n prod` to deploy to production
   - You can now see your game servers running in the Rivet dashboard. These will automatically scale up and down based on player demand.

**Connecting to the server**

1. Open the [Rivet Hub](https://hub.rivet.gg)
2. Navigate to _Your Game > API_
3. Under _Production_, click _Create Public Token_ and click the copy button
   ![Create public token](./docs/media/create_public_token.png)
4. Paste this token under the _Rivet Token_ field on the _Rivet API_ game object. This token should look like `pub_prod.XXXX`.
5. Click play

## Importing in to your own project

1. Install the following Unity assets
   - FishNet
   - Json.NET
   - ParrelSync (optional, helps with testing multiple clients)
2. Copy the following files
   - `Assets/Scripts/RivetManager.cs` (provides API endpoints for Rivet and manages client/server state)
   - `Assets/Scripts/RivetAuthenticator.cs` (integrates Rivet's authentication with FishNet,automatically created by `RivetManager`)
3. Create a game object in your scene with the `RivetManager` component
4. Call `_rivetManager.FindLobby(...)` to connect to a lobby (see `RivetUI.cs` for example)

