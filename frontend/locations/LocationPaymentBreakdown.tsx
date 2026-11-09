import { Box, Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import type { LocationDetailRange } from "@/locations/types";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Lists the accounts or funds that paid a location, largest first, with each one's share of the total.
 */
const LocationPaymentBreakdown = function ({
  title,
  shares,
  emptyMessage,
}: {
  readonly title: string;
  readonly shares: LocationDetailRange["accounts"];
  readonly emptyMessage: string;
}): JSX.Element {
  const total = shares.reduce((sum, share) => sum + share.amount, 0);
  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Typography variant="h5">{title}</Typography>
        {shares.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            {emptyMessage}
          </Typography>
        ) : (
          shares.map((share) => {
            const percent = total === 0 ? 0 : (share.amount / total) * 100;
            return (
              <Stack key={share.id} spacing={0.5}>
                <Stack
                  direction="row"
                  spacing={2}
                  sx={{ justifyContent: "space-between" }}
                >
                  <Typography variant="body2" noWrap>
                    {share.name}
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{ fontWeight: 600, whiteSpace: "nowrap" }}
                  >
                    {formatCurrency(share.amount)}{" "}
                    <Typography
                      component="span"
                      variant="body2"
                      color="text.secondary"
                    >
                      {Math.round(percent).toString()}%
                    </Typography>
                  </Typography>
                </Stack>
                <Box
                  sx={{
                    height: 6,
                    borderRadius: 3,
                    bgcolor: "action.hover",
                    overflow: "hidden",
                  }}
                >
                  <Box
                    sx={{
                      width: `${percent.toString()}%`,
                      height: "100%",
                      bgcolor: "primary.main",
                    }}
                  />
                </Box>
              </Stack>
            );
          })
        )}
      </Stack>
    </ContentSurface>
  );
};

export default LocationPaymentBreakdown;
