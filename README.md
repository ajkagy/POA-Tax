# POA-Tax

POA-Tax is a .NET CORE application that is built for POA Network based blockchains. It's a tax estimator tool to help individual validators. !!!This app only pulls mining rewards, it does not take into account fee rewards from mining!!!

# Prerequisites

1. Windows/Mac/Linux OS
2. Install the .NET CORE runtime libaries (See [Microsoft .NET Core Downloads](https://www.microsoft.com/net/download))

3. Parity Instance on POA Core network
(See [POA Installation](https://github.com/poanetwork/wiki/wiki/POA-Installation))

# Instructions
1. Open settings.json file and fill in the following infomation
    - RPC is defaulted to localhost. Leave this alone unless your rpc port is different for your local parity instance
    - Starting Block (optional): Leave empty string if unsure
    - Starting Date: starting date from which you want the app to run, format: MM-DD-YYYY
    - Mining Address: The Mining Address of the Validator
    - Tax Year: The year in which you want to pull transactions for mining rewards.

2. Start your local parity instance and let it fully sync. (It takes awhile for ancient blocks to sync in the background)
3. Build the program either in Visual Studio or run the provided release binaries.

# Run binaries from the Release
1. Download Binaries from Release (See [POA-Tax v2.1](https://github.com/ajkagy/POA-Tax/releases/tag/v2.1))
2. Open up a command prompt or bash prompt
3. Navigate to the folder where you download the binaries.
4. Run "dotnet POATax.dll" without the quotes

# Disclaimer

THIS SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESSED OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The Author of the application shall have no liability for the accuracy of the information the application produces and cannot be held liable for any third-party claims or losses of any damages. 
