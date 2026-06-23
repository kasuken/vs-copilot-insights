<p align="center">
  <img src="logo.png" width="140" alt="Copilot Insights logo" />
</p>

# Copilot Insights for Visual Studio

See your GitHub Copilot plan, quotas, reset window, and AI credit usage pacing directly inside Visual Studio.

## Overview

Copilot Insights gives you a fast, local view of the quota and entitlement data already associated with your GitHub Copilot account.

It focuses on practical visibility, not team analytics. It helps answer questions like:

- How many AI credits are left?
- When does quota reset?
- Am I likely to run out before reset?
- Is overage enabled, and what could it cost?
- Which organizations are providing Copilot access?

## Highlights

- Native Visual Studio tool window (`Copilot Insights`) with quota cards and plan details.
- AI credit health, mood, and remaining/used/total breakdown.
- Reset timing display with countdown and local reset date.
- Pacing guidance for daily, weekly, and workday usage.
- AI credit projection and daily capacity estimates by model cost (Efficient, Standard, Advanced).
- Overage messaging with estimated cost when overage is permitted.
- Local snapshot history with a mini trend chart and delta comparisons (since last refresh / yesterday).
- Weighted usage prediction (estimated days until exhaustion, confidence, sustainability).
- Burn-rate / trend analysis (recent vs overall, projected monthly cost).
- Configurable auto-refresh (background polling) and a custom AI credit limit.
- One-click export of the current snapshot to the clipboard as Markdown or raw JSON.
- Low-quota alert when AI credit usage crosses 85%.
- Manual refresh button from inside the tool window.

## What You Get

### Quota cards

Each quota card shows:

- Quota status and health badge.
- Limited vs unlimited handling.
- Remaining/used values and percentages.
- Over-quota details when applicable.

For AI Credits (`premium_interactions`), the extension also shows:

- Mood indicator and remaining percentage.
- Reset timing summary.
- Projected AI credits before reset.
- Daily capacity estimates by model cost multiplier.
- Overage policy summary.

### Reset timing and pacing

The tool window includes:

- Reset countdown (`days`/`hours`).
- Reset date converted to local time.
- Pacing guidance to stay within your AI credit quota:
  - Daily
  - Weekly
  - Workday (Mon-Fri)

### Plan details

A summary card with:

- Copilot plan
- Chat availability
- Access SKU
- Organization access list/count

### Usage trend and history

The tool window keeps a small local history of AI credit (`premium_interactions`) snapshots and uses it to show:

- A mini "AI Credits over time" bar chart of recent snapshots.
- Delta comparisons: change since the last refresh and since roughly a day ago.

### Prediction and burn rate

Once enough history is collected, the view adds:

- **Weighted prediction:** predicted daily usage, estimated daily/monthly cost, estimated days until exhaustion, a confidence level (low/medium/high), and a sustainability check against the reset date.
- **Burn rate analysis:** recent vs. overall burn rate, projected monthly cost, and an accelerating/slowing/stable trend indicator.

### Export

The `Export` card copies the current snapshot to the clipboard as:

- A Markdown summary for sharing in docs, issues, or chat.
- The raw Copilot payload as formatted JSON.

### Low-quota alert

When AI credit usage crosses 85% of your (plan or custom) limit, a one-time warning notification is shown for the current billing period.

### Settings

The `Settings` card persists locally and lets you configure:

- **Custom AI credit limit** — budget against a custom limit above your plan entitlement (`0` uses the plan default). Progress, prediction, pacing, and alerts then track the custom limit.
- **Auto-refresh interval (seconds)** — automatically refresh in the background every N seconds (`0` disables polling).

History, settings, and alert state are stored as JSON under `%LOCALAPPDATA%\vs-copilot-insights`. No external service is used.

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
