# -*- coding: utf-8 -*-
"""
Importa os lançamentos gerados (lancamentos_weber_tech_2018_2026.json) para a API
do SmartGest, um lançamento por vez, embrulhando cada registro em {"req": {...}}
conforme o endpoint espera.

USO:
    pip install requests
    python importar_lancamentos.py

Antes de rodar, ajuste as 2 variáveis abaixo: BASE_URL e ENDPOINT.
"""
import json
import time
import requests

# ---------------------------------------------------------------------------
# CONFIGURAÇÃO — ajuste aqui
# ---------------------------------------------------------------------------
BASE_URL = "http://localhost:8080"          # <- coloca a URL base da tua API
ENDPOINT = "/api/lancamentos"                   # <- ajusta a rota certa do endpoint

TOKEN = (
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9."
    "eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiSmV0aCBXZWJlciIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluaXN0cmFkb3IiLCJleHAiOjE3ODU5ODgwMTUsImlzcyI6IlNtYXJ0R2VzdC5BUEkiLCJhdWQiOiJTbWFydEdlc3QuRGVza3RvcCJ9."
    "UamH9H9Z-km6-g6xmwwb8Ra3faTTmbCRPcCj8LBcE3U"
)



INPUT_FILE = "lancamentos_weber_tech_2018_2026.json"
LOG_FILE = "importacao_erros.log"
DELAY_SEGUNDOS = 0.05   # pausa entre requests, pra não sobrecarregar a API (ajuste se precisar)

MODO_TESTE = True   # <- deixa True pra mandar só os primeiros 5 registros e validar antes de rodar tudo
QTD_TESTE = 100

# ---------------------------------------------------------------------------

def main():
    with open(INPUT_FILE, "r", encoding="utf-8") as f:
        lancamentos = json.load(f)

    if MODO_TESTE:
        lancamentos = lancamentos[:QTD_TESTE]

    total = len(lancamentos)
    sucesso = 0
    falhas = 0

    headers = {
        "Authorization": f"Bearer {TOKEN}",
        "Content-Type": "application/json",
    }

    url = BASE_URL.rstrip("/") + ENDPOINT

    with open(LOG_FILE, "w", encoding="utf-8") as log:
        for i, lancamento in enumerate(lancamentos, start=1):
            body = lancamento  # LancamentoRequest direto no body, sem wrapper, camelCase
            try:
                resp = requests.post(url, headers=headers, json=body, timeout=30)
                if resp.status_code in (200, 201):
                    sucesso += 1
                else:
                    falhas += 1
                    log.write(
                        f"[{i}/{total}] ref={lancamento.get('referenciaInterna')} "
                        f"status={resp.status_code} body={resp.text}\n"
                    )
            except requests.RequestException as e:
                falhas += 1
                log.write(
                    f"[{i}/{total}] ref={lancamento.get('referenciaInterna')} "
                    f"erro_conexao={e}\n"
                )

            if i % 100 == 0 or i == total:
                print(f"Progresso: {i}/{total}  (sucesso={sucesso}, falhas={falhas})")

            time.sleep(DELAY_SEGUNDOS)

    print("\nConcluído.")
    print(f"Sucesso: {sucesso}")
    print(f"Falhas:  {falhas}")
    if falhas:
        print(f"Detalhes das falhas em: {LOG_FILE}")


if __name__ == "__main__":
    main()
