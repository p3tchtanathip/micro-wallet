resource "google_compute_network" "vpc_network" {
  name                    = "microwallet-vpc"
  auto_create_subnetworks = false
}

resource "google_compute_subnetwork" "subnet" {
  name          = "microwallet-subnet"
  ip_cidr_range = "10.0.1.0/24"
  region        = var.region
  network       = google_compute_network.vpc_network.id
}

# Firewall rules for basic access
resource "google_compute_firewall" "allow_basic" {
  name    = "allow-basic-traffic"
  network = google_compute_network.vpc_network.name

  allow {
    protocol = "tcp"
    ports    = ["80", "22", "443", "6443"] # 6443 is for k3s API
  }

  source_ranges = ["0.0.0.0/0"]
}
