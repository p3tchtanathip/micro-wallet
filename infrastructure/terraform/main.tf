terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 5.0"
    }
  }
}

provider "google" {
  project = var.project_id
  region  = var.region
  zone    = var.zone
}

resource "google_project_service" "apis" {
  for_each = toset([
    "servicenetworking.googleapis.com",
    "redis.googleapis.com",
    "compute.googleapis.com",
    "sqladmin.googleapis.com",
    "artifactregistry.googleapis.com",
    "iam.googleapis.com",
    "iamcredentials.googleapis.com",
    "cloudresourcemanager.googleapis.com"
  ])

  service            = each.key
  disable_on_destroy = false
}
