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

/**
 * Type representing a Fund Amount.
 */
interface FundAmount {
  fund: Fund;
  amount: number;
}

export { type Fund, type CreateOrUpdateFundRequest, type FundAmount };
