import { decodeJwt } from "jose";

/**
 * Reads the expiration timestamp from a provider JWT without treating it as
 * verified authentication. The backend still validates the token completely.
 */
const getIdTokenExpiration = function (idToken: string): number | null {
  try {
    const expiration = decodeJwt(idToken).exp;
    return typeof expiration === "number" && Number.isFinite(expiration)
      ? expiration
      : null;
  } catch {
    return null;
  }
};

/**
 * Returns whether an ID token is no longer usable by the backend.
 */
const isIdTokenExpired = function (
  idToken: string,
  expiration?: number | null,
): boolean {
  const idTokenExpiration =
    typeof expiration === "number" ? expiration : getIdTokenExpiration(idToken);

  return (
    idTokenExpiration === null ||
    idTokenExpiration <= Math.floor(Date.now() / 1000)
  );
};

export { getIdTokenExpiration, isIdTokenExpired };
