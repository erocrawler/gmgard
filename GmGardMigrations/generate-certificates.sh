#!/bin/bash

# Generate X.509 certificates for OpenIddict production use (Linux/macOS)

# Variables
CERT_PATH="./Certificates"
SUBJECT_NAME="/CN=gmgard.openiddict"
FRIENDLY_NAME="GmGard OpenIddict"
VALID_DAYS=$((365 * 10))  # 10 years
CERT_PASSWORD=$(openssl rand -base64 16 | tr -d /=+ | cut -c1-16)  # Generate a random 16-character password

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Create Certificates directory if it doesn't exist
if [ ! -d "$CERT_PATH" ]; then
    mkdir -p "$CERT_PATH"
    echo "Created $CERT_PATH directory"
fi

echo "Generating self-signed certificate for OpenIddict..."

# Generate private key (2048-bit RSA)
openssl genrsa -out "$CERT_PATH/openiddict.key" 2048

# Generate certificate signing request with proper subject format
openssl req -new \
    -key "$CERT_PATH/openiddict.key" \
    -out "$CERT_PATH/openiddict.csr" \
    -subj "$SUBJECT_NAME"

# Generate self-signed certificate (valid for 10 years)
openssl x509 -req \
    -in "$CERT_PATH/openiddict.csr" \
    -signkey "$CERT_PATH/openiddict.key" \
    -out "$CERT_PATH/openiddict.crt" \
    -days $VALID_DAYS \
    -extfile <(printf "subjectAltName=DNS:gmgard.openiddict\nextendedKeyUsage=serverAuth")

# Export to PKCS#12 (.pfx) format for signing certificate
echo "Exporting signing certificate..."
openssl pkcs12 -export \
    -in "$CERT_PATH/openiddict.crt" \
    -inkey "$CERT_PATH/openiddict.key" \
    -out "$CERT_PATH/signing-cert.pfx" \
    -name "$FRIENDLY_NAME" \
    -passout pass:"$CERT_PASSWORD"

# Export to PKCS#12 (.pfx) format for encryption certificate (same cert)
echo "Exporting encryption certificate..."
openssl pkcs12 -export \
    -in "$CERT_PATH/openiddict.crt" \
    -inkey "$CERT_PATH/openiddict.key" \
    -out "$CERT_PATH/encryption-cert.pfx" \
    -name "$FRIENDLY_NAME" \
    -passout pass:"$CERT_PASSWORD"

# Clean up temporary files
rm "$CERT_PATH/openiddict.key" "$CERT_PATH/openiddict.csr"

echo ""
echo -e "${GREEN}✓ Certificates generated successfully!${NC}"
echo ""
echo "Certificate files created in: $CERT_PATH"
echo "  - signing-cert.pfx"
echo "  - encryption-cert.pfx"
echo ""
echo "Certificate details:"
echo "  Subject: $SUBJECT_NAME"
echo "  Validity: $VALID_DAYS days"
echo "  Expires: $(date -d "+10 years" '+%Y-%m-%d')"
echo ""
echo "Next steps:"
echo "1. Add certificate password to appsettings.Production.json:"
echo "   ApplicationSettings.OpenIddict.CertificatePassword = '$CERT_PASSWORD'"
echo ""
echo "2. Or set environment variable:"
echo "   export OPENIDDICT_CERT_PASSWORD='$CERT_PASSWORD'"
echo ""
echo -e "${YELLOW}⚠️  IMPORTANT:${NC}"
echo "- Keep the certificate password secure (use secrets management tools)"
echo "- Store certificates in a secure location with proper permissions"
echo "- Set permissions: chmod 600 $CERT_PATH/*.pfx"
echo "- Certificate expires on: $(date -d "+10 years" '+%Y-%m-%d')"
echo ""

# Set secure permissions on certificate files
chmod 600 "$CERT_PATH"/*.pfx

echo "Certificate permissions set to 600 (owner read/write only)"
