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
    # 1. Install k3s
    curl -sfL https://get.k3s.io | sh -s - --disable traefik

    # 2. Set KUBECONFIG for root (k3s default)
    export KUBECONFIG=/etc/rancher/k3s/k3s.yaml
    
    # Wait for k3s to be ready
    until kubectl get nodes; do sleep 5; done

    # 3. Install Helm
    curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

    # 4. Install NGINX Ingress Controller
    helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
    helm repo update
    helm upgrade --install ingress-nginx ingress-nginx \
      --repo https://kubernetes.github.io/ingress-nginx \
      --namespace ingress-nginx --create-namespace
  EOF
}
