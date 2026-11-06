export function getApiErrorMessage(err: unknown): string {
  const data = (err as { data?: unknown })?.data;

  // Identity errors (e.g. password rules): [{ code, description }, ...]
  if (Array.isArray(data)) {
    return (
      data
        .map((e) => e?.description)
        .filter(Boolean)
        .join("\n") || "Something went wrong."
    );
  }

  // Controller errors: { message: "..." }
  if (data && typeof data === "object" && "message" in data) {
    return String((data as { message: unknown }).message);
  }

  // No response body (offline, server down, timeout)
  return "Network error. Please try again.";
}