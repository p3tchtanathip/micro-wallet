resource "google_redis_instance" "cache" {
  name           = "microwallet-redis"
  tier           = "BASIC"
  memory_size_gb = 1
  region         = var.region
  location_id    = var.zone

  authorized_network = google_compute_network.vpc_network.id
  connect_mode       = "DIRECT_PEERING"

  redis_version = "REDIS_7_0"
  display_name  = "MicroWallet Redis Cache"
}
