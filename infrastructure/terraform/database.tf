resource "google_compute_global_address" "private_ip_address" {
  name          = "google-managed-services-vpc"
  purpose       = "VPC_PEERING"
  address_type  = "INTERNAL"
  prefix_length = 16
  network       = google_compute_network.vpc_network.id
}

resource "google_service_networking_connection" "private_vpc_connection" {
  network                 = google_compute_network.vpc_network.id
  service                 = "servicenetworking.googleapis.com"
  reserved_peering_ranges = [google_compute_global_address.private_ip_address.name]
}

resource "google_sql_database_instance" "postgres" {
  name             = "microwallet-postgres"
  database_version = "POSTGRES_15"
  region           = var.region
  depends_on       = [google_service_networking_connection.private_vpc_connection]

  settings {
    tier = "db-f1-micro"
    ip_configuration {
      ipv4_enabled    = false
      private_network = google_compute_network.vpc_network.id
    }
  }
  deletion_protection = false
}

resource "google_sql_user" "db_user" {
  name     = "postgres"
  instance = google_sql_database_instance.postgres.name
  password = "YourSecurePasswordHere" # You should change this or use a variable
}
