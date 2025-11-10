import {
  ActionColumnCell,
  ActionColumnHeader,
} from "@framework/listframe/ActionColumn";
import { AddCircleOutline, Delete, Info, ModeEdit } from "@mui/icons-material";
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { type Fund, getAllFunds } from "@data/FundRepository";
import {
  StringColumnCell,
  StringColumnHeader,
} from "@framework/listframe/StringColumn";
import { type JSX, useState } from "react";
import DeleteFundDialog from "@funds/DeleteFundDialog";
import FundDialog from "@funds/FundDialog";
import ModifyFundDialog from "@funds/ModifyFundDialog";
import { useQuery } from "@data/useQuery";

/**
 * Component that provides a list of funds and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of funds with various action buttons.
 */
const FundListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);

  const loadingRowCount = 10;
  const { data, isLoading, refetch } = useQuery<Fund[]>({
    queryFunction: getAllFunds,
    initialData: Array(loadingRowCount)
      .fill(null)
      .map((_, index) => ({ id: index.toString(), name: "", description: "" })),
  });

  return (
    <Box sx={{ paddingLeft: "25px" }}>
      <Typography
        variant="h6"
        sx={{ paddingBottom: "25px", paddingTop: "25px" }}
      >
        Funds
      </Typography>
      <Paper
        sx={{
          [`& .MuiIconButton-root`]: {
            display: "none",
          },
          [`& .MuiTableRow-hover:hover`]: {
            [`.MuiIconButton-root`]: {
              display: "inline-block",
            },
          },
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <StringColumnHeader label="Name" />
                <StringColumnHeader label="Description" />
                <ActionColumnHeader
                  icon={<AddCircleOutline />}
                  caption="Add"
                  onClick={(): void => {
                    setDialog(
                      <ModifyFundDialog
                        fund={null}
                        onClose={(): void => {
                          setDialog(null);
                          refetch();
                        }}
                      />,
                    );
                  }}
                />
              </TableRow>
            </TableHead>
            <TableBody>
              {data.map((row) => (
                <TableRow hover role="checkbox" tabIndex={-1} key={row.id}>
                  <StringColumnCell
                    label="Name"
                    value={row.name}
                    isLoading={isLoading}
                  />
                  <StringColumnCell
                    label="Description"
                    value={row.description}
                    isLoading={isLoading}
                  />
                  <ActionColumnCell
                    actions={[
                      {
                        icon: <Info />,
                        onClick: (): void => {
                          setDialog(
                            <FundDialog
                              fund={row}
                              onClose={(): void => {
                                setDialog(null);
                              }}
                            />,
                          );
                        },
                      },
                      {
                        icon: <ModeEdit />,
                        onClick: (): void => {
                          setDialog(
                            <ModifyFundDialog
                              fund={row}
                              onClose={(): void => {
                                setDialog(null);
                                refetch();
                              }}
                            />,
                          );
                        },
                      },
                      {
                        icon: <Delete />,
                        onClick: (): void => {
                          setDialog(
                            <DeleteFundDialog
                              fund={row}
                              onClose={(): void => {
                                setDialog(null);
                                refetch();
                              }}
                            />,
                          );
                        },
                      },
                    ]}
                    isLoading={isLoading}
                  />
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
      {dialog}
    </Box>
  );
};

export default FundListFrame;
