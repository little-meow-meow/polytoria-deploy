# PTDeploy
Continuous deployment of your project to Polytoria.

<a href="https://ko-fi.com/little_meow_meow"><img src="https://cdn.prod.website-files.com/5c14e387dab576fe667689cf/670f5a01c01ea9191809398c_support_me_on_kofi_blue.png" height="28px"></a>

> [!IMPORTANT]
> This project was not written with AI and does not accept AI-generated contributions.

## Setup
Create the file `.github/workflows/deploy.yml` in your repo:
```yml
name: Deploy

on:
  push:
    branches:
      - main

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v7
        with:
          path: ./project
      - uses: little-meow-meow/polytoria-deploy@main
        with:
          creator-token: ${{ secrets.CREATOR_TOKEN }}
          project-path: $GITHUB_WORKSPACE/project
          place-id: 123456  # Replace this!
```

Obtain a creator token from the Polytoria website:
- Visit [polytoria.com/create](https://polytoria.com/create)
- Open your browser's development tools to capture network requests
- On the page, click `Launch Creator 2.0`
- In the network inspector, view the response text for `polytoria.com/api/places/edit`
  - You may need to repeat this several times if your browser does not show response content.
  - If this still doesn't work, you may pull the creator token from the running process arguments.
    - Linux/Mac: `ps aux | grep -i "token"`
    - Windows: Use [System Informer](https://systeminformer.com/)
- Add the creator token as a *repository secret* named `CREATOR_TOKEN` in your project's repository settings.

### Advanced
You can configure your repository to deploy to different places from different branches.

Suppose you have two major branches, `main` and `stable`. You want to playtest changes you make before introducing those
changes to the main world. Using the following workflow, you can merge pull requests into `main` and, after playtesting,
merge `main` into `stable`.
```yml
name: Deploy

on:
  push:
    branches:
      - main
      - stable

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v7
        with:
          path: ./project
      - uses: little-meow-meow/polytoria-deploy@main
        with:
          creator-token: ${{ secrets.CREATOR_TOKEN }}
          project-path: $GITHUB_WORKSPACE/project
          place-id: ${{ github.ref_name == 'main' && '<PLAYTEST PLACE ID>' || '<MAIN PLACE ID>' }}  # Replace values
```
