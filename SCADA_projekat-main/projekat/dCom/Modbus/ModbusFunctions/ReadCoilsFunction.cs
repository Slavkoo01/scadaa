using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;


namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            byte[] ret_val = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, ret_val, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, ret_val, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, ret_val, 4, 2);
            ret_val[6] = CommandParameters.UnitId;
            ret_val[7] = CommandParameters.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)), 0, ret_val, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)), 0, ret_val, 10, 2);

            return ret_val;
        }
        /*        /*	//napravi recnik
	//petlja 1 za bajte od 0 do responese[8]
	//petlja 2 za bite od 0 do 8
	//value = shift/fja .net izvuci  bit -> short
	recnik.add(PointType.DO, paremtri.address, value)
	//ako ne ubacimo u recnik mozda nam i radi projekat 
	//ali ne mozemo to da vidimo 

    //da radimo analogno ne bismo imali 2 petlje nego bi isli od 0 do 20
    //samo ne bi smo radili ++ 
    //umesto shiftovanja i kako da pretvaramo u short na nama je da kako da 
    response[i] i response[i+1] pretvorimo u short
    //ima 6 klasa po 2 funkcije, deluje puno, ali kad jedno implementiramo 
    pack request za digital output onda cemo moci to isto da koristimo za 
    digital input. Sto se tice parse responsea, jedna od izmena kada zavrsimo ovu
    klasu u predjemo na sledecu koja je digital output je da promenimo samo 
    iz digital output u digital input, */
        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> r = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] != CommandParameters.FunctionCode + 0x80)
            {
                ushort start_address = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
                int count = 0;
                int byte_count = response[8];
                byte mask = 1;

                for (int i = 0; i < byte_count; i++)    //loop for bytes
                {
                    byte temp = response[9 + i];

                    for (int j = 0; j < 8; j++)     //loop for bits
                    {
                        ushort value = (ushort)(temp & mask);
                        Tuple<PointType, ushort> tuple = new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, start_address);
                        r.Add(tuple, value);

                        temp >>= 1;
                        count++;
                        start_address++;

                        ushort quantity = ((ModbusReadCommandParameters)CommandParameters).Quantity;
                        if (count >= quantity)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                HandeException(response[8]);
            }
            return r;
        }
    }
}