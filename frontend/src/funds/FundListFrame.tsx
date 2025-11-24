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
import { type JSX, useState } from "react";
import {
  StringColumnCell,
  StringColumnHeader,
} from "@framework/listframe/StringColumn";
import DeleteFundDialog from "@funds/DeleteFundDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
import FundDialog from "@funds/FundDialog";
import ModifyFundDialog from "@funds/ModifyFundDialog";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import useGetAllFunds from "@funds/useGetAllFunds";

/**
 * Component that provides a list of funds and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of funds with various action buttons.
 */
const FundListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const { funds, isLoading, error, refetch } = useGetAllFunds();

  const onAdd = function (): void {
    setDialog(
      <ModifyFundDialog
        fund={null}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Fund added successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };

  const onView = function (fund: Fund): void {
    setDialog(
      <FundDialog
        fund={fund}
        onClose={() => {
          setDialog(null);
        }}
      />,
    );
    setMessage(null);
  };

  const onEdit = function (fund: Fund): void {
    setDialog(
      <ModifyFundDialog
        fund={fund}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Fund updated successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };

  const onDelete = function (fund: Fund): void {
    setDialog(
      <DeleteFundDialog
        fund={fund}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Fund deleted successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };

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
                  onClick={onAdd}
                />
              </TableRow>
            </TableHead>
            <TableBody>
              {funds.map((fund) => (
                <TableRow hover role="checkbox" tabIndex={-1} key={fund.id}>
                  <StringColumnCell
                    label="Name"
                    value={fund.name}
                    isLoading={isLoading}
                    isError={error !== null}
                  />
                  <StringColumnCell
                    label="Description"
                    value={fund.description}
                    isLoading={isLoading}
                    isError={error !== null}
                  />
                  <ActionColumnCell
                    actions={[
                      {
                        key: "view",
                        icon: <Info />,
                        handleClick: (): void => {
                          onView(fund);
                        },
                      },
                      {
                        key: "edit",
                        icon: <ModeEdit />,
                        handleClick: (): void => {
                          onEdit(fund);
                        },
                      },
                      {
                        key: "delete",
                        icon: <Delete />,
                        handleClick: (): void => {
                          onDelete(fund);
                        },
                      },
                    ]}
                    isLoading={isLoading}
                    isError={error !== null}
                  />
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </Box>
  );
};

export default FundListFrame;
