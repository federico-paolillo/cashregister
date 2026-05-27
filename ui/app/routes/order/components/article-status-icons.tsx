import { DetailReceiptDisabledIcon } from "./detail-receipt-disabled-icon";
import { LowStockIcon } from "./low-stock-icon";

interface ArticleStatusIconsProps {
  lowQuantity: boolean;
  printDetailReceipt: boolean;
  className?: string;
}

export function ArticleStatusIcons({ lowQuantity, printDetailReceipt, className = "" }: ArticleStatusIconsProps) {
  if (!lowQuantity && printDetailReceipt) {
    return null;
  }

  return (
    <span className={`flex items-center gap-1 ${className}`}>
      {lowQuantity && <LowStockIcon />}
      {!printDetailReceipt && <DetailReceiptDisabledIcon />}
    </span>
  );
}
