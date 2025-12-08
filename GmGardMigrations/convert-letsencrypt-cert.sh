#!/bin/bash

# Convert Let's Encrypt certificates to PKCS#12 (.pfx) format for OpenIddict

# Variables
LETSENCRYPT_PATH="${1:-.}"  # Default to current directory
CERT_PATH="./Certificates"
DOMAIN="${2:-example.com}"
CERT_PASSWORD="${3:-}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Validate input files
if [ ! -f "$LETSENCRYPT_PATH/fullchain.pem" ] || [ ! -f "$LETSENCRYPT_PATH/privkey.pem" ]; then
    echo -e "${RED}✗ Error: fullchain.pem or privkey.pem not found in $LETSENCRYPT_PATH${NC}"
    echo "Usage: $0 <letsencrypt_path> <domain> [password]"
    echo ""
    echo "Example from Certbot (auto renewal):"
    echo "  $0 /etc/letsencrypt/live/example.com example.com 'your-password'"
    exit 1
fi

# Create Certificates directory if it doesn't exist
if [ ! -d "$CERT_PATH" ]; then
    mkdir -p "$CERT_PATH"
    echo "Created $CERT_PATH directory"
fi

echo "Converting Let's Encrypt certificates to PKCS#12 format..."
echo ""

# If no password provided, prompt for it
if [ -z "$CERT_PASSWORD" ]; then
    echo -e "${YELLOW}⚠️  No password provided. Using empty password (not recommended for production).${NC}"
    CERT_PASSWORD=""
fi

# Convert fullchain.pem + privkey.pem to signing-cert.pfx
echo "Creating signing certificate..."
openssl pkcs12 -export \
    -in "$LETSENCRYPT_PATH/fullchain.pem" \
    -inkey "$LETSENCRYPT_PATH/privkey.pem" \
    -out "$CERT_PATH/signing-cert.pfx" \
    -name "Let's Encrypt - $DOMAIN" \
    -password pass:"$CERT_PASSWORD"

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Failed to create signing certificate${NC}"
    exit 1
fi

# For encryption certificate, use the same cert (common for OpenIddict)
echo "Creating encryption certificate..."
openssl pkcs12 -export \
    -in "$LETSENCRYPT_PATH/fullchain.pem" \
    -inkey "$LETSENCRYPT_PATH/privkey.pem" \
    -out "$CERT_PATH/encryption-cert.pfx" \
    -name "Let's Encrypt - $DOMAIN" \
    -password pass:"$CERT_PASSWORD"

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Failed to create encryption certificate${NC}"
    exit 1
fi

# Set secure permissions
chmod 600 "$CERT_PATH"/*.pfx

echo ""
echo -e "${GREEN}✓ Certificates converted successfully!${NC}"
echo ""
echo "Certificate files created in: $CERT_PATH"
echo "  - signing-cert.pfx"
echo "  - encryption-cert.pfx"
echo ""
echo "Certificate details:"
echo "  Domain: $DOMAIN"
echo "  Issuer: Let's Encrypt"
echo ""
echo "Next steps:"
echo "1. Deploy the certificates to your production server:"
echo "   scp $CERT_PATH/*.pfx user@server:/path/to/gmgard/Certificates/"
echo ""
echo "2. Set secure permissions on the server:"
echo "   chmod 600 /path/to/gmgard/Certificates/*.pfx"
echo ""
echo "3. Configure password in appsettings.Production.json:"
echo "   \"OpenIddict\": {"
echo "     \"CertificatePassword\": \"$CERT_PASSWORD\""
echo "   }"
echo ""
echo "4. Or set environment variable:"
echo "   export OPENIDDICT_CERT_PASSWORD='$CERT_PASSWORD'"
echo ""
echo -e "${YELLOW}⚠️  IMPORTANT:${NC}"
echo "- Keep the certificate password secure"
echo "- Store certificates in a secure location"
echo "- Let's Encrypt certificates expire after 90 days"
echo "- Set up auto-renewal: certbot renew --quiet (in cron/systemd)"
echo "- After renewal, regenerate .pfx files using this script"
echo ""

# Show expiry date
echo "Certificate expiry:"
openssl x509 -enddate -noout -in "$LETSENCRYPT_PATH/fullchain.pem"
echo ""
