variable "project_id" {
  type    = string
  default = "microwallet-495518"
}

variable "region" {
  type    = string
  default = "asia-southeast1"
}

variable "zone" {
  type    = string
  default = "asia-southeast1-a"
}

variable "ssh_public_key_path" {
  type    = string
  default = "../../id_rsa_gcp.pub"
}

variable "db_password" {
  description = "The password for the database user"
  type        = string
  sensitive   = true
}
