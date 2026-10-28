import { Box, Stack, Typography } from "@mui/material";
import { Fragment, type JSX } from "react";
import {
  type OverviewAttentionGroup,
  describeAttentionGroup,
} from "@/overview/helpers";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
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
                  direction="row"
                  spacing={1}
                  sx={{
                    alignItems: "center",
                    p: 1.5,
                    minHeight: 44,
                    borderRadius: 2,
                    bgcolor: "action.hover",
                    "&:hover": { bgcolor: "action.selected" },
                  }}
                >
                  <Stack
                    direction={{ xs: "column", sm: "row" }}
                    spacing={{ xs: 1, sm: 2 }}
                    sx={{
                      alignItems: { xs: "stretch", sm: "center" },
                      flex: 1,
                      minWidth: 0,
                    }}
                  >
                    <Stack spacing={0.25} sx={{ flex: 1, minWidth: 0 }}>
                      <Typography sx={{ fontWeight: 500 }}>
                        {description.title}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {description.detail.map((part, index) => (
                          <Fragment key={index}>
                            {/* Lines only break between parts, with each separator leading the part after it */}
                            {index > 0 ? " " : null}
                            <Box
                              component="span"
                              sx={{
                                display: "inline-block",
                                maxWidth: "100%",
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                verticalAlign: "bottom",
                                whiteSpace: "nowrap",
                              }}
                            >
                              {index > 0 ? "· " : null}
                              {part.text}
                              {typeof part.amount === "string" ? (
                                <>
                                  {" "}
                                  <Box
                                    component="span"
                                    sx={{ color: "error.main" }}
                                  >
                                    {part.amount}
                                  </Box>
                                </>
                              ) : null}
                            </Box>
                          </Fragment>
                        ))}
                      </Typography>
                    </Stack>
                    <Typography
                      sx={{
                        fontWeight: 500,
                        whiteSpace: "nowrap",
                        flexShrink: 0,
                      }}
                    >
                      {description.amount}
                    </Typography>
                  </Stack>
                  <ChevronRight
                    aria-hidden
                    sx={{ color: "text.secondary", flexShrink: 0 }}
                  />
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
