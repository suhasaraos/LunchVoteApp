locals {
  common_tags = {
    Environment = var.env
    ManagedBy   = "Terraform"
    Project     = "LunchVoteApp"
    CostCenter  = "Engineering"
  }
}
