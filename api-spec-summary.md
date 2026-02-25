# API Spec Summary

This repo now has a maintained canonical spec for GoAffPro user/public APIs:

- `openapi/goaffpro-canonical.yaml`

## Key decisions

- Canonical source is local YAML maintained in this repo and used directly for client generation.
- Response envelopes are normalized with shared wrappers (`allOf`) for recurring pagination-like shapes.
- Drift-prone payloads keep `additionalProperties: true`.
- Known logical failures are captured with explicit error schemas/codes where the runtime behavior is stable.
- Known plain-text failure cases are modeled (`/user/feed/rewards` -> `404`, `/user/feed/products` -> `502`).

## Added/normalized user endpoints

- `/user/sites` -> `{ sites: [], count }` (fixed from upstream singular-object mismatch)
- `/user/stats/aggregate` -> `{ data: [] }`
- `/user/feed/orders|payouts|products|rewards|traffic` envelopes normalized
- `/user/commissions` supports observed variant payloads
- `/user/payouts/pending` added (observed working with bearer token)
- `/user/feed/transactions` added (currently returns `500` with bearer token but endpoint appears present)

## Probe scripts retained

- `scripts/analyze-api.ps1`  
  Scenario-based probe (auth/no-auth, body-aware, logical `200` error detection).
- `scripts/probe-openapi-spec.ps1`  
  Spec-driven probe for JSON/YAML OpenAPI files, nested shape summaries, optional no-auth user probes.

## Notes

- No access token is stored in repo files.
- Probe outputs are intentionally not kept as committed artifacts.
