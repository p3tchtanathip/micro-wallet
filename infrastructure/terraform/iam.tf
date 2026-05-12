# 1. Create Identity Pool
resource "google_iam_workload_identity_pool" "github_pool" {
  workload_identity_pool_id = "github-actions-pool"
  display_name              = "GitHub Actions Pool"
}

# 2. Create Identity Provider for GitHub
resource "google_iam_workload_identity_pool_provider" "github_provider" {
  workload_identity_pool_id          = google_iam_workload_identity_pool.github_pool.workload_identity_pool_id
  workload_identity_pool_provider_id = "github-provider"
  attribute_mapping = {
    "google.subject"       = "assertion.sub"
    "attribute.repository" = "assertion.repository"
    "attribute.owner"      = "assertion.repository_owner"
  }
  attribute_condition = "assertion.repository_owner == 'p3tchtanathip'"
  oidc {
    issuer_uri = "https://token.actions.githubusercontent.com"
  }
}

# 3. Allow GitHub Repository to assume the Service Account
resource "google_service_account_iam_member" "wif_user" {
  service_account_id = "projects/${var.project_id}/serviceAccounts/terraform-sa@${var.project_id}.iam.gserviceaccount.com"
  role               = "roles/iam.workloadIdentityUser"
  member             = "principalSet://iam.googleapis.com/${google_iam_workload_identity_pool.github_pool.name}/attribute.repository/p3tchtanathip/micro-wallet"
}
