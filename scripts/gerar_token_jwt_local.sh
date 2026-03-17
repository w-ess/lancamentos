#!/usr/bin/env bash
set -euo pipefail

issuer="${Autenticacao__Issuer:-fluxodecaixa-local}"
audience="${Autenticacao__Audience:-fluxodecaixa-clientes}"
chave="${Autenticacao__ChaveAssinatura:-}"
subject="${JWT_SUBJECT:-usuario-local}"
expiracao_em_minutos="${Autenticacao__ExpiracaoEmMinutos:-60}"

if [[ -z "$chave" ]]; then
  echo "Defina a variavel de ambiente Autenticacao__ChaveAssinatura com pelo menos 32 bytes." >&2
  exit 1
fi

tamanho_chave="$(printf '%s' "$chave" | wc -c | tr -d '[:space:]')"
if (( tamanho_chave < 32 )); then
  echo "A chave configurada em Autenticacao__ChaveAssinatura precisa ter ao menos 32 bytes." >&2
  exit 1
fi

json_escape() {
  printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g'
}

base64url() {
  openssl base64 -A | tr '+/' '-_' | tr -d '='
}

if (( $# == 0 )); then
  permissoes=(
    "lancamentos.escrita"
    "lancamentos.leitura"
    "consolidado.leitura"
  )
else
  permissoes=("$@")
fi

scope="$(printf '%s ' "${permissoes[@]}")"
scope="${scope% }"

agora_epoch="$(date +%s)"
expira_epoch="$((agora_epoch + (expiracao_em_minutos * 60)))"
jti="$(cat /proc/sys/kernel/random/uuid)"

header='{"alg":"HS256","typ":"JWT"}'
payload="$(printf \
  '{"iss":"%s","aud":"%s","sub":"%s","iat":%s,"nbf":%s,"exp":%s,"jti":"%s","scope":"%s"}' \
  "$(json_escape "$issuer")" \
  "$(json_escape "$audience")" \
  "$(json_escape "$subject")" \
  "$agora_epoch" \
  "$agora_epoch" \
  "$expira_epoch" \
  "$jti" \
  "$(json_escape "$scope")")"

header_codificado="$(printf '%s' "$header" | base64url)"
payload_codificado="$(printf '%s' "$payload" | base64url)"
assinatura="$(printf '%s' "${header_codificado}.${payload_codificado}" | openssl dgst -binary -sha256 -hmac "$chave" | base64url)"

printf '%s.%s.%s\n' "$header_codificado" "$payload_codificado" "$assinatura"
