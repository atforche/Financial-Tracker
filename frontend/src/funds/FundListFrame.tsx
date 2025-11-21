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
import { type JSX, useCallback, useState } from "react";
import {
  StringColumnCell,
  StringColumnHeader,
} from "@framework/listframe/StringColumn";
import DeleteFundDialog from "@funds/DeleteFundDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import FundDialog from "@funds/FundDialog";
import ModifyFundDialog from "@funds/ModifyFundDialog";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import { useQuery } from "@data/useQuery";

/**
 * Component that provides a list of funds and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of funds with various action buttons.
 */
const FundListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const loadingRowCount = 10;
  const { data, isLoading, error, refetch } = useQuery<Fund[]>({
    queryFunction: getAllFunds,
    initialData: Array(loadingRowCount)
      .fill(null)
      .map((_, index) => ({ id: index.toString(), name: "", description: "" })),
  });

  const onAddFinish = useCallback(
    (success: boolean): void => {
      setDialog(null);
      if (success) {
        setMessage("Fund added successfully.");
      }
      refetch();
    },
    [refetch],
  );
  const onAdd = useCallback((): void => {
    setDialog(<ModifyFundDialog fund={null} onClose={onAddFinish} />);
    setMessage(null);
  }, [onAddFinish]);

  const onViewClose = useCallback((): void => {
    setDialog(null);
  }, []);
  const onView = useCallback(
    (row: Fund): void => {
      setDialog(<FundDialog fund={row} onClose={onViewClose} />);
      setMessage(null);
    },
    [onViewClose],
  );

  const onEditFinish = useCallback(
    (success: boolean): void => {
      setDialog(null);
      if (success) {
        setMessage("Fund updated successfully.");
      }
      refetch();
    },
    [refetch],
  );
  const onEdit = useCallback(
    (row: Fund): void => {
      setDialog(<ModifyFundDialog fund={row} onClose={onEditFinish} />);
      setMessage(null);
    },
    [onEditFinish],
  );

  const onDeleteFinish = useCallback(
    (success: boolean): void => {
      setDialog(null);
      if (success) {
        setMessage("Fund deleted successfully.");
      }
      refetch();
    },
    [refetch],
  );
  const onDelete = useCallback(
    (row: Fund): void => {
      setDialog(<DeleteFundDialog fund={row} onClose={onDeleteFinish} />);
      setMessage(null);
    },
    [onDeleteFinish],
  );

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
              {data.map((row) => (
                <TableRow hover role="checkbox" tabIndex={-1} key={row.id}>
                  <StringColumnCell
                    label="Name"
                    value={row.name}
                    isLoading={isLoading}
                    isError={error !== null}
                  />
                  <StringColumnCell
                    label="Description"
                    value={row.description}
                    isLoading={isLoading}
                    isError={error !== null}
                  />
                  <ActionColumnCell
                    actions={[
                      {
                        key: "view",
                        icon: <Info />,
                        handleClick: (): void => {
                          onView(row);
                        },
                      },
                      {
                        key: "edit",
                        icon: <ModeEdit />,
                        handleClick: (): void => {
                          onEdit(row);
                        },
                      },
                      {
                        key: "delete",
                        icon: <Delete />,
                        handleClick: (): void => {
                          onDelete(row);
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
