output "wif_provider_name" {
  value       = google_iam_workload_identity_pool_provider.github_provider.name
  description = "Copy this to GitHub Secret: WIF_PROVIDER"
}

output "artifact_registry_repo" {
  value       = google_artifact_registry_repository.microwallet_repo.name
}

output "gce_public_ip" {
  value       = google_compute_instance.k3s_node.network_interface[0].access_config[0].nat_ip
  description = "Copy this to GitHub Secret: GCE_VM_IP"
}

output "db_private_ip" {
  value       = google_sql_database_instance.postgres.private_ip_address
  description = "IP address of the database (Use this in your Connection String)"
}
