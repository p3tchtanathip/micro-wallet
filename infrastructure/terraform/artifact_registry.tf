resource "google_artifact_registry_repository" "microwallet_repo" {
  location      = var.region
  repository_id = "microwallet-repo"
  description   = "Docker repository for MicroWallet app"
  format        = "DOCKER"
}
