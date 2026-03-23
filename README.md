<p align="center">
  <img src="logo.png" width="140" alt="Copilot Insights logo" />
</p>

# Copilot Insights for Visual Studio

See your GitHub Copilot plan, quotas, reset window, and premium usage pacing directly inside Visual Studio.

## Overview

Copilot Insights gives you a fast, local view of the quota and entitlement data already associated with your GitHub Copilot account.

It focuses on practical visibility, not team analytics. It helps answer questions like:

- How much premium quota is left?
- When does quota reset?
- Am I likely to run out before reset?
- Is overage enabled, and what could it cost?
- Which organizations are providing Copilot access?

## Highlights

- Native Visual Studio tool window (`Copilot Insights`) with quota cards and plan details.
- Premium quota health, mood, and remaining/used/total breakdown.
- Reset timing display with countdown and local reset date.
- Pacing guidance for daily, weekly, and workday usage.
- Premium projection and capacity estimates by model cost (`0.33x`, `1x`, `3x`).
- Overage messaging with estimated cost when overage is permitted.
- Manual refresh button from inside the tool window.

## What You Get

### Quota cards

Each quota card shows:

- Quota status and health badge.
- Limited vs unlimited handling.
- Remaining/used values and percentages.
- Over-quota details when applicable.

For `premium_interactions`, the extension also shows:

- Mood indicator and remaining percentage.
- Reset timing summary.
- Projection before reset.
- Daily capacity estimates by model cost multiplier.
- Overage policy summary.

### Reset timing and pacing

The tool window includes:

- Reset countdown (`days`/`hours`).
- Reset date converted to local time.
- Pacing guidance to stay within premium quota:
  - Daily
  - Weekly
  - Workday (Mon-Fri)

### Plan details

A summary card with:

- Copilot plan
- Chat availability
- Access SKU
- Organization access list/count

## Requirements

- Visual Studio 2022 (with support for `Microsoft.VisualStudio.Extensibility`)
- **GitHub CLI (`gh`) installed and available on PATH**
- GitHub account with Copilot access

## Important: GitHub CLI authentication

This extension retrieves your token via:

```sh
gh auth token
```

Before using the extension, authenticate with GitHub CLI:

```sh
gh auth login
```

If `gh` is missing or not authenticated, the extension cannot fetch Copilot data.

## Installation and Run (Development)

1. Clone this repository.
2. Open `vs-copilot-insights.slnx` in Visual Studio.
3. Restore/build the solution.
4. Start the extension debug session (experimental instance) from Visual Studio.
5. Open **Extensions** menu and run **Copilot Insights** command.

## Data Source and Privacy

The extension calls:

- `https://api.github.com/copilot_internal/user`

using your local GitHub CLI auth token.

No external service is used by this project to store your Copilot quota data.

## Troubleshooting

### Could not obtain a GitHub token

- Ensure GitHub CLI is installed: `gh --version`
- Ensure you are logged in: `gh auth status`
- If needed, run: `gh auth login`

### API errors (403/404)

- Your account/org/tenant may not expose this Copilot endpoint.
- The endpoint is internal and may change over time.

### No data in the tool window

- Confirm your GitHub account has Copilot access.
- Use the in-window **Refresh** button.
- Verify your network and GitHub authentication state.

## License

[MIT](LICENSE)
