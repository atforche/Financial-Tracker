import {
  type OverviewAttentionGroup,
  describeAttentionGroup,
} from "@/overview/helpers";
import { Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the OverviewAttentionList component.
 */
interface OverviewAttentionListProps {
  readonly groups: readonly OverviewAttentionGroup[];
  readonly currentPeriodId: string | null;
}

/**
 * Lists each group of items needing attention as one line linking to where it can be resolved.
 */
const OverviewAttentionList = function ({
  groups,
  currentPeriodId,
}: OverviewAttentionListProps): JSX.Element {
  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Typography variant="h5">Needs attention</Typography>
        {groups.length === 0 ? (
          <Typography color="text.secondary">
            Nothing needs attention right now.
          </Typography>
        ) : (
          groups.map((group) => {
            const description = describeAttentionGroup(group, currentPeriodId);
            return (
              <Link
                key={group.kind}
                href={description.href}
                style={{ textDecoration: "none", color: "inherit" }}
              >
                <Stack
                  direction={{ xs: "column", sm: "row" }}
                  spacing={{ xs: 1, sm: 2 }}
                  sx={{
                    alignItems: { xs: "stretch", sm: "center" },
                    p: 1.5,
                    minHeight: 44,
                    borderRadius: 2,
                    bgcolor: "action.hover",
                    "&:hover": { bgcolor: "action.selected" },
                  }}
                >
                  <Stack spacing={0.25} sx={{ flex: 1, minWidth: 0 }}>
                    <Typography sx={{ fontWeight: 500 }}>
                      {description.title}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {description.detail}
                    </Typography>
                  </Stack>
                  <Stack
                    direction={{ xs: "row", sm: "column" }}
                    spacing={{ xs: 2, sm: 0.25 }}
                    sx={{
                      alignItems: { xs: "center", sm: "flex-end" },
                      justifyContent: "space-between",
                      flexShrink: 0,
                    }}
                  >
                    <Typography sx={{ fontWeight: 500, whiteSpace: "nowrap" }}>
                      {description.amount}
                    </Typography>
                    <Typography variant="body2" color="primary">
                      {description.actionLabel}
                    </Typography>
                  </Stack>
                </Stack>
              </Link>
            );
          })
        )}
      </Stack>
    </ContentSurface>
  );
};

export default OverviewAttentionList;
