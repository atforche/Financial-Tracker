import { Box, Stack, Typography } from "@mui/material";
import { Fragment, type JSX } from "react";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
import Link from "next/link";
import type { Route } from "next";

/**
 * One class of problem, with the amount it covers and where it can be resolved.
 */
interface AttentionItem {
  readonly title: string;
  readonly amount: string;
  readonly items: readonly { text: string; amount?: string }[];
  readonly remainingItemCount: number;
  readonly facts: readonly string[];
  readonly href: Route;
}

/**
 * Lists each class of problem needing attention as one line linking to where it can be resolved.
 */
const AttentionList = function ({
  descriptions,
}: {
  readonly descriptions: readonly AttentionItem[];
}): JSX.Element {
  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Typography variant="h5">Needs attention</Typography>
        {descriptions.length === 0 ? (
          <Typography color="text.secondary">
            Nothing needs attention right now.
          </Typography>
        ) : (
          descriptions.map((description) => (
            <Link
              key={description.title}
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
                    {description.items.length === 0 ? null : (
                      <Typography variant="body2" color="text.secondary">
                        {[
                          ...description.items,
                          ...(description.remainingItemCount > 0
                            ? [
                                {
                                  text: `and ${description.remainingItemCount} more`,
                                },
                              ]
                            : []),
                        ].map((item, index, items) => (
                          <Fragment key={index}>
                            {/* Lines only break between items, and each comma stays with the item before it */}
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
                              {item.text}
                              {"amount" in item &&
                              typeof item.amount === "string" ? (
                                <>
                                  {" "}
                                  <Box
                                    component="span"
                                    sx={{ color: "error.main" }}
                                  >
                                    {item.amount}
                                  </Box>
                                </>
                              ) : null}
                              {index < items.length - 1 ? "," : null}
                            </Box>
                          </Fragment>
                        ))}
                      </Typography>
                    )}
                    {description.facts.map((fact) => (
                      <Typography
                        key={fact}
                        variant="caption"
                        color="text.secondary"
                      >
                        {fact}
                      </Typography>
                    ))}
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
          ))
        )}
      </Stack>
    </ContentSurface>
  );
};

export type { AttentionItem };
export default AttentionList;
