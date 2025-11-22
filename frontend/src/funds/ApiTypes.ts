import type { components } from "@data/api";

/**
 * Type representing a Fund.
 */
type Fund = components["schemas"]["FundModel"];

/**
 * Type representing a request to create or update a Fund.
 */
type CreateOrUpdateFundRequest =
  components["schemas"]["CreateOrUpdateFundModel"];

export { type Fund, type CreateOrUpdateFundRequest };
