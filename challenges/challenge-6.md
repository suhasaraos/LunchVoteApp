# Challenge 6: 🚢 Ship It Like a Pro

### *"Production-grade deployment isn't just shipping code  it's shipping confidence"*

**Duration:** ~60 minutes

[⬅ Back to Hackathon Participant Guide](../HACKATHON_PARTICIPANT_GUIDE.md)

---

### Synopsis

Your app is deployed, secured, and connected to a real database. But is it production-ready? Not yet. Real-world applications need **zero-downtime deployments**, **rollback capabilities**, and **deployment verification** before going live. In this final challenge, you'll level up your deployment game by implementing **Blue/Green Deployment** using Azure App Service **Deployment Slots**  a feature that lets you deploy and test new versions without affecting live users.

### What You'll Learn

#### Blue/Green Deployment
**Blue/Green Deployment** is a release strategy that eliminates downtime and reduces risk:

- **Blue** = the current production version (live traffic)
- **Green** = the new version you're about to release (staging)

The process:
1. Deploy the new version to the **staging slot** (green)
2. Test and verify the staging slot independently
3. **Swap** the staging and production slots  this is nearly instant (typically < 1 second)
4. If something goes wrong, swap back  instant rollback!

The swap operation works by changing the routing rules, not moving files, making it extraordinarily fast.

#### Azure App Service Deployment Slots
**Deployment Slots** are live App Service instances with their own hostnames. The **Standard (S1)** tier or above supports slots. Key features:

- **Isolated environments**  Staging slot has its own URL, app settings, and connection strings
- **Warm-up**  Azure warms up the staging slot before swapping, preventing cold-start issues
- **Auto-swap**  Optionally auto-swap to production after deploying to staging
- **Traffic routing**  Gradually route a percentage of traffic to the new version (canary deployments)
- **Slot-specific settings**  Some settings can be "sticky" to a slot (e.g., different database for staging vs production)

> ⚠️ **Important: Free (F1) Tier Does Not Support Deployment Slots**
>
> The Terraform templates deploy the App Service Plan on the **F1 Free** tier (a single shared plan for both the API and SPA). The Free tier **does not** support deployment slots. You **must** upgrade to at least **Standard S1** before you can create a staging slot.
>
> **Upgrade via Azure CLI (recommended for this challenge):**
> ```powershell
> # Get the plan name from Terraform output
> cd infra/terraform
> $PLAN_NAME = terraform output -raw service_plan_name
>
> # Upgrade to Standard S1
> az appservice plan update --name $PLAN_NAME --resource-group rg-lunchvote-dev --sku S1
> ```
>
> Alternatively, update the `sku_name` in your Terraform `app-service` module from `"F1"` to `"S1"` and run `terraform apply`.

#### App Service Scaling & Monitoring
Beyond deployment, you'll explore:
- **Application logging**  View real-time logs from your App Service using `az webapp log tail`
- **Health checks**  Configure App Service health checks to automatically replace unhealthy instances
- **App Service metrics**  View request count, response time, and error rates in the Azure Portal

### Your Mission

Upgrade your App Service Plan from Free (F1) to Standard (S1), create a staging deployment slot, implement a blue/green deployment workflow, and demonstrate zero-downtime releases.

### 🤖 GitHub Copilot Skill Focus: Code Review + Commit Message Generation

For the final challenge, you'll use Copilot as a **code reviewer** and **DevOps assistant** - the capstone of your Copilot journey.

#### AI-Powered Code Review

Before committing your Terraform changes for deployment slots:

1. Open the **Source Control** view (`Ctrl+Shift+G`)
2. Click the **Code Review** button (sparkle icon) to review all uncommitted changes
3. Copilot will add inline review comments highlighting potential issues, security concerns, and improvement suggestions
4. Alternatively, select a block of changed code → right-click → **Generate Code** → **Review**

#### Smart Commit Messages

Instead of writing vague commit messages like "updated stuff":
1. Stage your changes in Source Control
2. Click the **sparkle icon ✨** next to the commit message input box
3. Copilot analyzes your diff and generates a descriptive commit message like: *"feat: add staging deployment slot with warm-up config and S1 SKU upgrade"*

#### Using `#changes` in Chat

Reference your current uncommitted changes in chat:
- *"#changes Review my Terraform changes for the deployment slot and suggest any security improvements"*
- *"#changes Summarize what I changed and help me write release notes"*

#### Quick Chat for DevOps Questions

Use **Quick Chat** (`Ctrl+Shift+Alt+L`) for fast questions without leaving your current workflow:
- *"What's the difference between slot swap and slot swap with preview?"*
- *"How does App Service warm-up work during a slot swap?"*

### Acceptance Criteria

| # | Criteria | Details |
|---|----------|---------|
| ✅ 1 | **Tier upgraded** | The shared App Service Plan (used by both API and SPA) is upgraded from **F1 Free** to at least **Standard S1** tier. Use Azure CLI: `az appservice plan update --name <PLAN_NAME> --resource-group <RG> --sku S1`, or update the `sku_name` in Terraform and apply |
| ✅ 2 | **Staging slot created** | A deployment slot named `staging` exists on the backend API App Service |
| ✅ 3 | **Deploy to staging** | Deploy a new version of the API to the **staging** slot (not production). The staging slot should have its own URL (e.g., `https://app-lunchvote-api-dev-xxx-staging.azurewebsites.net`) |
| ✅ 4 | **Verify staging** | Access the staging slot's Swagger UI or API endpoint and confirm it's running correctly |
| ✅ 5 | **Swap slots** | Perform a slot swap between staging and production. Verify that the production URL now serves the new version |
| ✅ 6 | **Zero downtime** | Demonstrate that the swap did not cause any downtime  the production URL remained accessible throughout the swap |
| ✅ 7 | **Rollback capability** | Perform another swap to roll back to the previous version. Verify the rollback works |
| ✅ 8 | **Live log streaming** | Use `az webapp log tail` to stream live logs from the production App Service and demonstrate you can see real-time request/response activity |
| ✅ 9 | **Terraform updated** | Your Terraform code is updated to include the staging slot configuration and the upgraded SKU |
| ✅ 10 | **GitHub Copilot** | Used GitHub Copilot to help with slot management commands, Terraform slot configuration, or understanding deployment slot concepts |

### Useful Commands

> 💡 Use GitHub Copilot to help construct and troubleshoot these commands.

```powershell
# Get the App Service Plan name from Terraform output
cd infra/terraform
$PLAN_NAME = terraform output -raw service_plan_name

# Upgrade App Service Plan to Standard S1 (required for deployment slots)
az appservice plan update --name $PLAN_NAME --resource-group <RG> --sku S1

# Create a staging deployment slot
az webapp deployment slot create --name <APP_NAME> --resource-group <RG> --slot staging

# Deploy to the staging slot
az webapp deploy --name <APP_NAME> --resource-group <RG> --slot staging --src-path ./publish.zip --type zip

# Browse the staging slot
az webapp browse --name <APP_NAME> --resource-group <RG> --slot staging

# Swap staging to production
az webapp deployment slot swap --name <APP_NAME> --resource-group <RG> --slot staging --target-slot production

# Rollback (swap again)
az webapp deployment slot swap --name <APP_NAME> --resource-group <RG> --slot staging --target-slot production

# Stream live logs
az webapp log tail --name <APP_NAME> --resource-group <RG>

# View deployment slot list
az webapp deployment slot list --name <APP_NAME> --resource-group <RG> -o table
```

### Terraform Hint for Deployment Slots

Ask GitHub Copilot to help you add this to your App Service module:

```hcl
resource "azurerm_linux_web_app_slot" "staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.main.id
  # ... configure same settings as production
}
```
