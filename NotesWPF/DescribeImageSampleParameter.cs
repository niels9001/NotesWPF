using NotesWPF.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NotesWPF
{
    internal class SampleNavigationParameters
    {
        private bool shouldWaitForCompletion;

        public CancellationToken CancellationToken { get; private set; }
        public string ModelPath => @"C:\Users\nielslaute\models\microsoft--Phi-3-vision-128k-instruct-onnx-cpu\main\cpu-int4-rtn-block-32-acc-level-4";
        public HardwareAccelerator HardwareAccelerator => HardwareAccelerator.CPU;

        public bool ShouldWaitForCompletion
        {
            [MemberNotNullWhen(true, nameof(SampleLoadedCompletionSource))]
            get
            {
                return shouldWaitForCompletion;
            }
        }

        public TaskCompletionSource<bool>? SampleLoadedCompletionSource { get; set; }

        public SampleNavigationParameters(CancellationToken loadingCanceledToken)
        {
            CancellationToken = loadingCanceledToken;
        }

        public void RequestWaitForCompletion()
        {
            shouldWaitForCompletion = true;
            SampleLoadedCompletionSource = new TaskCompletionSource<bool>();
        }

        public void NotifyCompletion()
        {
            SampleLoadedCompletionSource?.SetResult(true);
        }
    }
}