using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public class MellatPolicyRulesOptions
{
    public OperationRules? CommonRules { get; set; }

    public OperationRules? CustomerInquiry { get; set; }
    public OperationRules? CustomerInquiryResult { get; set; }
    public OperationRules? ContractFileNoCollateral { get; set; }
    public OperationRules? ContractFileWithCollateral { get; set; }
    public OperationRules? OtpRequest { get; set; }
    public OperationRules? PayRequest { get; set; }
    public OperationRules? PayResponse { get; set; }
    public OperationRules? Installments { get; set; }
    public OperationRules? DepositRequest { get; set; }
    public OperationRules? RepaymentRequest { get; set; }
    public OperationRules? CustomerBilling { get; set; }
    public OperationRules? PurchaseDetails { get; set; }
    public OperationRules? TransferRegister { get; set; }
    public OperationRules? ReturnedTransfers { get; set; }
}
