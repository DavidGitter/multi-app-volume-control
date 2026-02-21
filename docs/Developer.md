# Developer Guide

This guide is intended for developers of this project.

## Branches

- `main` : always stable, only receives merges from `release/*` branches
- `dev` : integration branch, all features merge here first
- `feature/[issue-number]-short-description` : one branch per feature or fix, optional issue number after the `/` (for example: `feature/42-changelog-ui`)

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/) format for consistency:

```
feat: add changelog UI to settings form
fix: correct quoted path in registry Run key
```

## Creating a Release

1. Create a release **branch** from `dev`: `git checkout -b release/1.x.x dev`
2. **Bump** the `CURRENT_VERSION` field in `Form1.cs` of the mavc-target-ui-win to the new **version**
3. **Build Agent and UI** solutions in Release configuration
4. Navigate "Deployment Project Properties" of the MavcSetup module (hit f4) and **update** the "**Version**" field to the new version.
> [!IMPORTANT] This must be done, otherwise the user will get an error later when updating, as the MSI versions do not differ.

5. **Build MavcSetup** to generate `MavcSetup.msi` in `...\mavc\mavc-setup\Release`
6. **Zip** the `MavcSetup.msi`
7. **Commit** to release branch with a message like `chore: release version 1.x.x` and **push** to origin
8. Open a **PR** from `release/1.x.x` into `main` and get it approved
9. **Merge** the PR into `main`
10. **Tag** the merge commit on `main`
11. **Create** a new **GitHub Release** from that tag on `main`
    - Mark it as Latest for main releases
    - Attach the zipped `MavcSetup.msi`
    - Write release notes using the following structure:

```markdown
# Release of version 1.x.x

## Changes
<user-facing features and fixes>

## Release changes
<installer/setup/packaging changes>

## Disclaimer
This project is provided as is, without warranty of any kind, express or implied, including but not limited to
the warranties of merchantability, fitness for a particular purpose, and non-infringement.

In no event shall the authors or copyright holders be liable for any claim, damages, or other liability,
whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the project
or the use or other dealings in the project.
```

After the release is published:

1. **Merge** `release/1.x.x` **back** into `dev` **to** keep it in **sync**
2. **Delete** the release branch: `git branch -d release/1.x.x`