﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Abstractions.SimpleMemory;
using Lombiq.Arithmetics;


namespace Hast.Samples.SampleAssembly
{
    public class Posit32FusedCalculator
    {
        public const int CalculateFusedSum_InputPosit32CountIndex = 0;
        public const int CalculateFusedSum_InputPosit32StartIndex = 1;
        public const int CalculateFusedSum_OutputPosit32Index = 0;
        public const int CalculateFusedDotProduct_InputPosit32CountIndex = 0;
        public const int CalculateFusedDotProduct_InputPosit32sStartIndex = 1;
        public const int CalculateFusedDotProduct_OutputPosit32Index = 2;

        public const int MaxArrayChunkSize = 200;
        public const int MaxInputArraySize = 1000000;
        public const int QuireSizeIn32BitChunks = Posit32.QuireSize >> 5;
        public const int QuireSizeIn64BitChunks = Posit32.QuireSize >> 6;

        public virtual void CalculateFusedSum(SimpleMemory memory)
        {
            uint numberCount = memory.ReadUInt32(CalculateFusedSum_InputPosit32CountIndex);
            var posit32ArrayChunk = new Posit32[MaxArrayChunkSize];

            var quireStartingValue = (Quire)new Posit32(0);
            var batchCount = numberCount / MaxArrayChunkSize;
            if (numberCount % MaxArrayChunkSize != 0)
            {
                batchCount += 1;
            }
            for (int i = 0; i < batchCount; i++)
            {
                for (var j = 0; j < posit32ArrayChunk.Length; j++)
                {
                    if (i * MaxArrayChunkSize + j < numberCount)
                    {
                        posit32ArrayChunk[j] = new Posit32(memory.ReadUInt32(CalculateFusedSum_InputPosit32StartIndex + i * MaxArrayChunkSize + j), true);
                    }
                    else posit32ArrayChunk[j] = new Posit32(0);
                }
                quireStartingValue = Posit32.FusedSum(posit32ArrayChunk, quireStartingValue);
            }
            var result = new Posit32(quireStartingValue);
            memory.WriteUInt32(CalculateFusedSum_OutputPosit32Index, result.PositBits);
        }
    }

    public static class Posit32FusedCalculatorExtensions
    {
        public static float CalculateFusedSum(this Posit32FusedCalculator posit32FusedCalculator, uint[] posit32Array)
        {
            if (posit32Array.Length > Posit32FusedCalculator.MaxInputArraySize)
            {
                throw new IndexOutOfRangeException("The maximum number of posits to be summed with the fused sum operation can not exceed the MaxInPutArraySize specified in the Posit32FusedCalculator class.");
            }
            var memory = new SimpleMemory(Posit32FusedCalculator.MaxInputArraySize + 1);

            memory.WriteUInt32(Posit32FusedCalculator.CalculateFusedSum_InputPosit32CountIndex, (uint)posit32Array.Length);

            for (var i = 0; i < posit32Array.Length; i++)
            {
                memory.WriteUInt32(Posit32FusedCalculator.CalculateFusedSum_InputPosit32StartIndex + i, posit32Array[i]);
            }

            posit32FusedCalculator.CalculateFusedSum(memory);

            return (float)new Posit32(memory.ReadUInt32(Posit32FusedCalculator.CalculateFusedSum_OutputPosit32Index), true);
        }

        public static readonly string[] ManuallySizedArrays = new[]
        {
           "System.UInt64[] Lombiq.Arithmetics.Quire::Segments()",
           "Lombiq.Arithmetics.Quire Lombiq.Arithmetics.Quire::op_Addition(Lombiq.Arithmetics.Quire,Lombiq.Arithmetics.Quire).array",
           "Lombiq.Arithmetics.Quire Lombiq.Arithmetics.Quire::op_RightShift(Lombiq.Arithmetics.Quire,System.Int32).array",
           "Lombiq.Arithmetics.Quire Lombiq.Arithmetics.Quire::op_LeftShift(Lombiq.Arithmetics.Quire,System.Int32).array"
        };
    }
}
