﻿using System.Text;
using Newtonsoft.Json;

namespace bf_bot.TO
{
    public class UpdateInstructionReport
    {
        [JsonProperty(PropertyName = "status")]
        public InstructionReportStatus Status { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public InstructionReportErrorCode ErrorCode { get; set; }

        [JsonProperty(PropertyName = "instruction")]
        public UpdateInstruction Instruction { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0}", "UpdateInstructionReport")
                        .AppendFormat(" : Status={0}", Status)
                        .AppendFormat(" : ErrorCode={0}", ErrorCode)
                        .AppendFormat(" : Instruction={0}", Instruction);

            return sb.ToString();
        }
    }
}
