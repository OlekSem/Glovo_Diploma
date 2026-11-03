# Glovo Diploma

## Company moderation flow

Partner company submissions now start in `Pending` status. Only `Approved` companies are returned by the normal partner company endpoint; partners can track all of their applications, including cancellation notes, at `/company-requests`.

`AdminFront` is a separate Vite + React + Tailwind portal. It uses the protected `/api/admin` endpoints to list, filter, review, approve, or cancel company applications. Run it locally with `cd AdminFront && npm install && npm run dev`, or use the `admin-frontend` service in `compose.yaml` (port `8082`).

At startup the API creates the `Administrator` role. To provision the first login, set `Admin:Email` and `Admin:Password` in environment variables or application settings; optional `Admin:Username`, `Admin:FirstName`, and `Admin:LastName` customize the account. No public admin registration or password-reset routes are exposed.

### Moderation API

- `POST /api/admin/auth/login` — body: `{ "identifier", "password" }`
- `GET /api/admin/companies?status=Pending|Approved|Canceled`
- `GET /api/admin/companies/{id}`
- `POST /api/admin/companies/{id}/approve` — optional `{ "comment" }`
- `POST /api/admin/companies/{id}/cancel` — optional `{ "comment" }`

All company moderation endpoints require an authenticated user in the `Administrator` role. The migration preserves existing companies as `Approved`; all future submissions default to `Pending`.
