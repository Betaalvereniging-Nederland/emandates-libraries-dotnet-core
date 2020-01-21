using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Library.MessageBuilders
{
    using p09 = XML.Schemas.pain009;
    using p10 = XML.Schemas.pain010;
    using p11 = XML.Schemas.pain011;

    internal class EMandateMessageBuilder
    {
        private IConfiguration Configuration { get; set; }
        private Instrumentation LocalInstrumentCode { get; set; }

        public EMandateMessageBuilder(IConfiguration configuration, Instrumentation instrumentCode)
        {
            this.Configuration = configuration;
            this.LocalInstrumentCode = instrumentCode;
        }

        private DateTime Now()
        {
            return DateTime.UtcNow;
        }

        private void VerifyMaxAmount(decimal? maxAmount)
        {
            if (!maxAmount.HasValue)
                return;

            if (LocalInstrumentCode == Instrumentation.B2B)
            {
                if (Math.Abs((double)maxAmount) <= 2 * Double.Epsilon)
                {
                    throw new CommunicatorException("MaxAmount can't be 0");
                }
                var val = maxAmount.Value.ToString(CultureInfo.InvariantCulture);
                string[] arr = val.Split('.');
                if (arr.Length > 1 && arr[1].Length > 2)
                {
                    throw new CommunicatorException("no more than 2 decimal places allowed for MaxAmount");
                }
                val = val.Replace(".", "");
                if (maxAmount.Value.ToString(CultureInfo.InvariantCulture).Length > 11)
                {
                    throw new CommunicatorException("MaxAmount should have maximum 11 digits (excluding decimal separator)");
                }
            }
        }

        public string GetNewMandate(NewMandateRequest newMandateRequest)
        {
            VerifyMaxAmount(newMandateRequest.MaxAmount);

            var eMandate = new p09.Document
            {
                MndtInitnReq = new p09.MandateInitiationRequestV04
                {
                    GrpHdr = new p09.GroupHeader47
                    {
                        MsgId = newMandateRequest.MessageId,
                        CreDtTm = Now(),
                    },
                    Mndt = new[]
                    {
                        new p09.Mandate7
                        {
                            MndtId = newMandateRequest.EMandateId,
                            MndtReqId = "NOTPROVIDED",
                            Tp = new p09.MandateTypeInformation1
                            {
                                SvcLvl = new p09.ServiceLevel8Choice
                                {
                                    ItemElementName = p09.ItemChoiceType4.Cd,
                                    Item = "SEPA"
                                },
                                LclInstrm = new p09.LocalInstrument2Choice
                                {
                                    ItemElementName = p09.ItemChoiceType5.Cd,
                                    Item = Enum.GetName(typeof(Instrumentation), this.LocalInstrumentCode).ToUpper()
                                }
                            },
                            Ocrncs = new p09.MandateOccurrences3
                            {
                                SeqTp = (p09.SequenceType2Code) newMandateRequest.SequenceType
                                // TODO: not allowed yet
                                //Frqcy = new p09.Frequency21Choice
                                //{
                                //    Item = new p09.FrequencyPeriod1
                                //    {
                                //        Tp = p09.Frequency6Code.YEAR,
                                //        CntPerPrd = 1.0
                                //    }
                                //}
                            },
                            MaxAmt = (this.LocalInstrumentCode == Instrumentation.B2B && newMandateRequest.MaxAmount.HasValue)?
                                new p09.ActiveCurrencyAndAmount { Ccy = "EUR", Value = newMandateRequest.MaxAmount.Value } : null,
                            Rsn = newMandateRequest.EMandateReason != null? new p09.MandateSetupReason1Choice
                            {
                                ItemElementName = p09.ItemChoiceType6.Prtry,
                                Item = newMandateRequest.EMandateReason != null? newMandateRequest.EMandateReason : String.Empty
                            } : null,
                            Cdtr = new p09.PartyIdentification43
                            {
                            },
                            Dbtr = new p09.PartyIdentification43
                            {
                                Id = newMandateRequest.DebtorReference != null? new p09.Party11Choice
                                {
                                    Item = new p09.PersonIdentification5()
                                    {
                                        Othr = new[]
                                        {
                                            new p09.GenericPersonIdentification1
                                            {
                                                Id = newMandateRequest.DebtorReference
                                            }
                                        }
                                    }
                                } : null,
                            },
                            DbtrAgt = new p09.BranchAndFinancialInstitutionIdentification5
                            {
                                FinInstnId = new p09.FinancialInstitutionIdentification8
                                {
                                    BICFI = newMandateRequest.DebtorBankId
                                }
                            },
                            RfrdDoc = newMandateRequest.PurchaseId != null? new []
                            {
                                new p09.ReferredDocumentInformation6
                                {
                                    Tp = new p09.ReferredDocumentType4
                                    {
                                        CdOrPrtry = new p09.ReferredDocumentType3Choice
                                        {
                                            Item = newMandateRequest.PurchaseId
                                        }
                                    }
                                }
                            } : null,
                        }
                    }
                }
            };

            return ProcessDateTimes(eMandate.Serialize());
        }

        public string GetAmend(AmendmentRequest amendmentRequest)
        {
            var eMandate = new p10.Document
            {
                MndtAmdmntReq = new p10.MandateAmendmentRequestV04
                {
                    GrpHdr = new p10.GroupHeader47
                    {
                        MsgId = amendmentRequest.MessageId,
                        CreDtTm = Now(),
                    },
                    UndrlygAmdmntDtls = new []
                    {
                        new p10.MandateAmendment4
                        {
                            AmdmntRsn = new p10.MandateAmendmentReason1
                            {
                                Rsn = new p10.MandateReason1Choice
                                {
                                    ItemElementName = p10.ItemChoiceType4.Cd,
                                    Item = "MD16"
                                }
                            },
                            OrgnlMndt = new p10.OriginalMandate3Choice
                            {
                                Item = new p10.Mandate5
                                {
                                    MndtId = amendmentRequest.EMandateId,
                                    //MndtReqId = "NOTPROVIDED",
                                    Cdtr = new p10.PartyIdentification43(),
                                    Dbtr = new p10.PartyIdentification43(),
                                    DbtrAcct = new p10.CashAccount24
                                    {
                                        Id = new p10.AccountIdentification4Choice
                                        {
                                            Item = amendmentRequest.OriginalIban
                                        }
                                    },
                                    DbtrAgt = new p10.BranchAndFinancialInstitutionIdentification5
                                    {
                                        FinInstnId = new p10.FinancialInstitutionIdentification8
                                        {
                                            BICFI = amendmentRequest.OriginalDebtorBankId
                                        }
                                    }
                                }
                            },
                            Mndt = new p10.Mandate6
                            {
                                MndtId = amendmentRequest.EMandateId,
                                MndtReqId = "NOTPROVIDED",
                                Tp = new p10.MandateTypeInformation1
                                {
                                    SvcLvl = new p10.ServiceLevel8Choice
                                    {
                                        ItemElementName = p10.ItemChoiceType5.Cd,
                                        Item = "SEPA"
                                    },
                                    LclInstrm = new p10.LocalInstrument2Choice
                                    {
                                        ItemElementName = p10.ItemChoiceType6.Cd,
                                        Item = Enum.GetName(typeof(Instrumentation), this.LocalInstrumentCode).ToUpper()
                                    },
                                },
                                Ocrncs = new p10.MandateOccurrences3
                                {
                                    SeqTp = (p10.SequenceType2Code) amendmentRequest.SequenceType
                                    // TODO: not allowed
                                    //Frqcy = new p10.Frequency21Choice
                                    //{
                                    //    Item = new p10.FrequencyPeriod1
                                    //    {
                                    //        Tp = p10.Frequency6Code.YEAR
                                    //        CntPerPrd = 1.0m
                                    //    }
                                    //}
                                },
                                Rsn = amendmentRequest.EMandateReason != null? new p10.MandateSetupReason1Choice
                                {
                                    ItemElementName = p10.ItemChoiceType7.Prtry,
                                    Item = amendmentRequest.EMandateReason ?? string.Empty
                                } : null,
                                Cdtr = new p10.PartyIdentification43
                                {
                                },
                                Dbtr = new p10.PartyIdentification43
                                {
                                    Id = amendmentRequest.DebtorReference != null? new p10.Party11Choice
                                    {
                                        Item = new p10.PersonIdentification5
                                        {
                                            Othr = new []
                                            {
                                                new p10.GenericPersonIdentification1
                                                {
                                                    Id = amendmentRequest.DebtorReference
                                                }
                                            }
                                        }
                                    } : null,
                                },
                                DbtrAgt = new p10.BranchAndFinancialInstitutionIdentification5
                                {
                                    FinInstnId = new p10.FinancialInstitutionIdentification8
                                    {
                                        BICFI = amendmentRequest.DebtorBankId
                                    }
                                },
                                RfrdDoc = amendmentRequest.PurchaseId != null? new []
                                {
                                    new p10.ReferredDocumentInformation6 
                                    {
                                        Tp = new p10.ReferredDocumentType4
                                        {
                                            CdOrPrtry = new p10.ReferredDocumentType3Choice
                                            {
                                                Item = amendmentRequest.PurchaseId
                                            }
                                        }
                                    }
                                } : null
                            }
                        }
                    }
                }
            };

            return ProcessDateTimes(eMandate.Serialize());
        }

        public string GetCancel(CancellationRequest cancellationRequest)
        {
            VerifyMaxAmount(cancellationRequest.MaxAmount);

            var eMandate = new p11.Document
            {
                MndtCxlReq = new p11.MandateCancellationRequestV04
                {
                    GrpHdr = new p11.GroupHeader47
                    {
                        CreDtTm = Now(),
                        MsgId = cancellationRequest.MessageId
                    },
                    UndrlygCxlDtls = new []
                    {
                        new p11.MandateCancellation4
                        {
                            CxlRsn = new p11.PaymentCancellationReason1
                            {
                                Rsn = new p11.MandateReason1Choice
                                {
                                    ItemElementName = p11.ItemChoiceType4.Cd,
                                    Item = "MD16"
                                }
                            },
                            OrgnlMndt = new p11.OriginalMandate3Choice
                            {
                                Item = new p11.Mandate5
                                {
                                    MndtId = cancellationRequest.EMandateId,
                                    MndtReqId = "NOTPROVIDED",
                                    Tp = new p11.MandateTypeInformation1
                                    {
                                        SvcLvl = new p11.ServiceLevel8Choice
                                        {
                                            ItemElementName = p11.ItemChoiceType5.Cd,
                                            Item = "SEPA"
                                        },
                                        LclInstrm = new p11.LocalInstrument2Choice
                                        {
                                            ItemElementName = p11.ItemChoiceType6.Cd,
                                            Item = Enum.GetName(typeof(Instrumentation), this.LocalInstrumentCode).ToUpper()
                                        }
                                    },
                                    Ocrncs = new p11.MandateOccurrences3
                                    {
                                        SeqTp = (p11.SequenceType2Code) cancellationRequest.SequenceType,
                                        //Frqcy = new p11.Frequency21Choice
                                        //{
                                        //    Item = new p11.FrequencyPeriod1
                                        //    {
                                        //        Tp = p11.Frequency6Code.YEAR,
                                        //        CntPerPrd = 1.0
                                        //    }
                                        //}
                                    },
                                    MaxAmt = (cancellationRequest.MaxAmount.HasValue)?
                                        new p11.ActiveOrHistoricCurrencyAndAmount { Ccy = "EUR", Value = cancellationRequest.MaxAmount.Value } : null,
                                    Rsn = cancellationRequest.EMandateReason != null? new p11.MandateSetupReason1Choice
                                    {
                                        ItemElementName = p11.ItemChoiceType7.Prtry,
                                        Item = cancellationRequest.EMandateReason != null? cancellationRequest.EMandateReason : String.Empty
                                    } : null,
                                    Cdtr = new p11.PartyIdentification43
                                    {
                                    },
                                    Dbtr = new p11.PartyIdentification43
                                    {
                                        Id = cancellationRequest.DebtorReference != null? new p11.Party11Choice
                                        {
                                            Item = new p11.PersonIdentification5
                                            {
                                                Othr = new []
                                                {
                                                    new p11.GenericPersonIdentification1
                                                    {
                                                        Id = cancellationRequest.DebtorReference
                                                    }
                                                }
                                            }
                                        } : null,
                                    },
                                    DbtrAcct = new p11.CashAccount24
                                    {
                                        Id = new p11.AccountIdentification4Choice
                                        {
                                            Item = cancellationRequest.OriginalIban
                                        }
                                    },
                                    DbtrAgt = new p11.BranchAndFinancialInstitutionIdentification5
                                    {
                                        FinInstnId = new p11.FinancialInstitutionIdentification8
                                        {
                                            BICFI = cancellationRequest.DebtorBankId
                                        }
                                    },
                                    RfrdDoc = cancellationRequest.PurchaseId != null? new []
                                    {
                                        new p11.ReferredDocumentInformation6
                                        {
                                            Tp = new p11.ReferredDocumentType4
                                            {
                                                CdOrPrtry = new p11.ReferredDocumentType3Choice
                                                {
                                                    Item = cancellationRequest.PurchaseId
                                                }
                                            }
                                        }
                                    }: null
                                }
                            }
                        }
                    }
                }
            };

            return ProcessDateTimes(eMandate.Serialize());
        }

        private string ProcessDateTimes(string input)
        {
            string[] dateTimeElementNames =
            {
                "CreDtTm", "RltdDt", "FrDt", "ToDt", "FrstColltnDt", "FnlColltnDt", "BirthDt",
            };

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);

            foreach (var elementName in dateTimeElementNames)
            {
                foreach (XmlElement element in doc.GetElementsByTagName(elementName, "*"))
                {
                    var existing = DateTime.Parse(element.InnerText, CultureInfo.InvariantCulture);
                    var newval = existing.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'");
                    element.InnerText = newval;
                }
            }

            return doc.OuterXml;
        }
    }
}
