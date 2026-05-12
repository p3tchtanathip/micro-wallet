resource "google_service_account" "k3s_sa" {
  account_id   = "k3s-node-sa"
  display_name = "Service Account for k3s Node"
}

resource "google_project_iam_member" "ar_reader" {
  project = var.project_id
  role    = "roles/artifactregistry.reader"
  member  = "serviceAccount:${google_service_account.k3s_sa.email}"
}

resource "google_compute_instance" "k3s_node" {
  name         = "k3s-master"
  machine_type = "e2-medium"
  zone         = var.zone

  boot_disk {
    initialize_params {
      image = "ubuntu-os-cloud/ubuntu-2204-lts"
      size  = 30
    }
  }

  network_interface {
    subnetwork = google_compute_subnetwork.subnet.id
    access_config {}
  }

  service_account {
    email  = google_service_account.k3s_sa.email
    scopes = ["cloud-platform"]
  }

  metadata = {
    ssh-keys = "ubuntu:${file(var.ssh_public_key_path)}"
  }

  metadata_startup_script = <<-EOF
    #!/bin/bash
    curl -sfL https://get.k3s.io | sh -s - --disable traefik
  EOF
}
