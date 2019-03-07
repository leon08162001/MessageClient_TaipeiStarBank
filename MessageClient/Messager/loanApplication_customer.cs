using System;

namespace MessageClient.Messager
{
    public partial class loanApplication_customer
    {
        public decimal pk { get; set; }
        public string order_nbr { get; set; }
        public string country { get; set; }
        public int seq { get; set; }
        public string customer_type { get; set; }
        public string nickname { get; set; }
        public string eng_name { get; set; }
        public Nullable<bool> is_pay { get; set; }
        public string id_num { get; set; }
        public Nullable<System.DateTime> date_of_birth { get; set; }
        public string phone_mobile { get; set; }
        public string phone_home { get; set; }
        public string phone_office { get; set; }
        public string add_1 { get; set; }
        public string add_2 { get; set; }
        public string refno { get; set; }
        public string lst_upd_user { get; set; }
        public Nullable<System.DateTime> lst_upd_date { get; set; }
        public Nullable<bool> is_shareholder { get; set; }
        public string shareholder_contributions { get; set; }
        public string shareholder_contributions_shares { get; set; }
    }
}